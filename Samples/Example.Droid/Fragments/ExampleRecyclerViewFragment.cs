using System;
using System.Windows.Input;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Platform.WeakSubscription;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Droid.Support.V4;
using Example.Core.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Shared.Attributes;

namespace Example.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("example.droid.fragments.ExampleRecyclerViewFragment")]
    public class ExampleRecyclerViewFragment : BaseFragment<ExampleRecyclerViewModel>
    {
        private IDisposable _itemSelectedToken;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            var recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.my_recycler_view);
            if (recyclerView != null)
            {
                // User our custom adapter
                // Doing this will make things break on call on
                // MyCustomAdapter::OnCreateViewHolder because MyCustomAdapter::BindingContext
                // is null...
                var adapter = new MyCustomAdapter {ItemCustomClick = new MvxCommand(DoItemCustomClick)};

                recyclerView.Adapter = adapter;
                recyclerView.HasFixedSize = true;
                var layoutManager = new LinearLayoutManager(Activity);
                recyclerView.SetLayoutManager(layoutManager);
            }

            _itemSelectedToken = ViewModel.WeakSubscribe(() => ViewModel.SelectedItem,
                (sender, args) => {
                    if (ViewModel.SelectedItem != null)
                        Toast.MakeText(Activity,
                            $"Selected: {ViewModel.SelectedItem.Title}",
                            ToastLength.Short).Show();
                });

            var swipeToRefresh = view.FindViewById<MvxSwipeRefreshLayout>(Resource.Id.refresher);
            var appBar = Activity.FindViewById<AppBarLayout>(Resource.Id.appbar);
            if (appBar != null)
                appBar.OffsetChanged += (sender, args) => swipeToRefresh.Enabled = args.VerticalOffset == 0;

            return view;
        }

        private void DoItemCustomClick()
        {
            // This is never executed because of NullReference on ViewHolder creation
            Toast.MakeText(Activity, "Here I am", ToastLength.Short).Show();
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            _itemSelectedToken.Dispose();
            _itemSelectedToken = null;
        }

        protected override int FragmentId => Resource.Layout.fragment_example_recyclerview;

        ////////////////////////////////////////////////////////////////////////
        // Let's create a custom adapter to use a custom ViewHolder (in order
        // to add custom bindings for example).
        ////////////////////////////////////////////////////////////////////////
        public class MyCustomAdapter : MvxRecyclerAdapter
        {
            public ICommand ItemCustomClick { get; set; }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                //////////////////////////////////////////////////////////////////////////////////////
                // Here is the NullReference exception : BindingContext is null
                var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);

                return new MyCustomViewHolder(InflateViewForHolder(parent, viewType, itemBindingContext), itemBindingContext)
                {
                    Click = ItemClick,
                    LongClick = ItemLongClick,
                    CustomClick = ItemCustomClick
                };
            }

            
        }
    }

    public class MyCustomViewHolder : MvxRecyclerViewHolder
    {
        public MyCustomViewHolder(View itemView, MvxAndroidBindingContext bindingContext)
            : base(itemView, bindingContext)
        {
        }

        private bool _customClickDefined;
        private ICommand _customClick;

        public ICommand CustomClick
        {
            get
            {
                return _customClick;
            }

            set
            {
                _customClick = value;
                if (null != _customClick && !_customClickDefined)
                {
                    _customClickDefined = true;
                    ///////////////////////////////////////////////////////////////////////////
                    // Concrete sample of why I would want to customize the ViewHolder: adding 
                    // custom click binding on a given view
                    ///////////////////////////////////////////////////////////////////////////

                    //var view = ItemView.FindViewById<ImageView>(Resource.Id.MyCustomImage);
                    //if (null != view)
                    //    view.CustomClickEvent = CustomClick;
                }
            }
        }
    }
}