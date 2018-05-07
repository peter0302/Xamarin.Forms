﻿using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellSearchViewAdapter : BaseAdapter, IFilterable
	{
		public const string DoNotUpdateMarker = "__DO_NOT_UPDATE__";

		private readonly SearchHandler _searchHandler;
		private readonly IShellContext _shellContext;
		private DataTemplate _defaultTemplate;

		public ShellSearchViewAdapter(SearchHandler searchHandler, IShellContext shellContext)
		{
			_searchHandler = searchHandler ?? throw new ArgumentNullException(nameof(searchHandler));
			_shellContext = shellContext ?? throw new ArgumentNullException(nameof(shellContext));
			SearchController.ListProxyChanged += OnListPropxyChanged;
			_searchHandler.PropertyChanged += OnSearchHandlerPropertyChanged;
		}

		private Filter _filter;

		public Filter Filter => _filter ?? (_filter = new CustomFilter(this));

		public override int Count => ListProxy.Count;

		private IReadOnlyList<object> _emptyList = new List<object>();
		private IReadOnlyList<object> ListProxy => SearchController.ListProxy ?? _emptyList;

		private DataTemplate DefaultTemplate
		{
			get
			{
				if (_defaultTemplate == null)
				{
					_defaultTemplate = new DataTemplate(() =>
					{
						var label = new Label();
						label.SetBinding(Label.TextProperty, _searchHandler.DisplayMemberName ?? ".");
						label.HorizontalTextAlignment = TextAlignment.Center;
						label.VerticalTextAlignment = TextAlignment.Center;

						return label;
					});
				}
				return _defaultTemplate;
			}
		}

		private ISearchHandlerController SearchController => _searchHandler;

		public override Java.Lang.Object GetItem(int position)
		{
			return new ObjectWrapper(ListProxy[position]);
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override AView GetView(int position, AView convertView, ViewGroup parent)
		{
			var item = ListProxy[position];

			ContainerView result = null;
			if (convertView != null)
			{
				result = convertView as ContainerView;
				result.View.BindingContext = item;
			}
			else
			{
				var template = _searchHandler.ItemTemplate ?? DefaultTemplate;
				var view = (View)template.CreateContent(item, _shellContext.Shell);
				view.BindingContext = item;
				view.Platform = _shellContext.Shell.Platform;

				result = new ContainerView(parent.Context, view);
			}

			return result;
		}

		protected virtual void OnSearchHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SearchHandler.ItemTemplateProperty.PropertyName)
			{
				NotifyDataSetChanged();
			}
		}

		private void OnListPropxyChanged(object sender, ListProxyChangedEventArgs e)
		{
			NotifyDataSetChanged();
		}

		private class CustomFilter : Filter
		{
			private readonly BaseAdapter _adapter;

			public CustomFilter(BaseAdapter adapter)
			{
				_adapter = adapter;
			}

			protected override FilterResults PerformFiltering(ICharSequence constraint)
			{
				var results = new FilterResults();

				results.Count = 100;
				return results;
			}

			protected override void PublishResults(ICharSequence constraint, FilterResults results)
			{
				_adapter.NotifyDataSetChanged();
			}
		}

		private class ObjectWrapper : Java.Lang.Object
		{
			public ObjectWrapper(object obj)
			{
				Object = obj;
			}

			private object Object { get; set; }

			public override string ToString() => DoNotUpdateMarker;
		}
	}
}