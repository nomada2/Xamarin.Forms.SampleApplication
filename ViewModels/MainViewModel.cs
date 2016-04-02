﻿using Prism.Commands;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SampleApplication
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IRepository _repository;

        private bool _listRefreshing;

        private SubscriptionToken _modelUpdatedEventToken;

        private ObservableCollection<SampleItem> _sampleItems;

        private SampleItem _selectedSampleItem;

        public MainViewModel(IRepository repository)
        {
            _repository = repository;
            FetchSampleItemsCommand = DelegateCommand.FromAsyncHandler(FetchSampleItemsAsync);
            OpenSelectedSampleItemCommand = DelegateCommand.FromAsyncHandler(OpenSelectedSampleItemAsync);
            CreateSampleItemNavigationCommand = DelegateCommand.FromAsyncHandler(CreateSampleItemNavigateAsync);
            Title = "Sample Application For Xamarin Forms";
        }

        public ICommand CreateSampleItemNavigationCommand { get; private set; }
        public ICommand FetchSampleItemsCommand { get; private set; }

        public bool ListRefreshing
        {
            get { return _listRefreshing; }
            set { SetProperty(ref _listRefreshing, value); }
        }

        public ICommand OpenSelectedSampleItemCommand { get; private set; }

        public ObservableCollection<SampleItem> SampleItems
        {
            get { return _sampleItems; }
            set { SetProperty(ref _sampleItems, value); }
        }

        public SampleItem SelectedSampleItem
        {
            get { return _selectedSampleItem; }
            set { SetProperty(ref _selectedSampleItem, value); }
        }

        public string Title { get; set; }

        public override void Closing()
        {
            CC.EventMessenger.GetEvent<ModelUpdatedMessageEvent<SampleItem>>().Unsubscribe(_modelUpdatedEventToken);
        }

        public override async Task InitializeAsync(System.Collections.Generic.Dictionary<string, string> args)
        {
            _modelUpdatedEventToken = CC.EventMessenger.GetEvent<ModelUpdatedMessageEvent<SampleItem>>().Subscribe(OnSampleItemUpdated);
            await FetchSampleItemsAsync();
        }

        private async Task CreateSampleItemNavigateAsync()
        {
            await Navigation.NavigateAsync(Constants.Navigation.ItemPage);
        }

        private async Task FetchSampleItemsAsync()
        {
            ListRefreshing = true;

            try
            {
                FetchModelCollectionResult<SampleItem> fetchResult = await _repository.FetchSampleItemsAsync();

                if (fetchResult.IsValid())
                {
                    SampleItems = fetchResult.ModelCollection.AsObservableCollection();

                    ListRefreshing = false;
                }
                else
                {
                    ListRefreshing = false;
                    await CC.UserNotifier.ShowMessageAsync(fetchResult.Notification.ToString(), "Fetch Sample Items Failed");
                }
            }
            finally
            {
                ListRefreshing = false;
            }
        }

        private void OnSampleItemUpdated(ModelUpdatedMessageResult<SampleItem> updateResult)
        {
            SampleItems.UpdateCollection(updateResult.UpdatedModel, updateResult.UpdateEvent);
        }

        private async Task OpenSelectedSampleItemAsync()
        {
            if (SelectedSampleItem != null)
            {
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    {Constants.Parameters.Id, SelectedSampleItem.Id}
                };

                await Navigation.NavigateAsync(Constants.Navigation.ItemPage, args);
            }
        }
    }
}