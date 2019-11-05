using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Common.UI.Views {
    public class RepeatedView<TModel> : View<IPresenter<RepeatedView<TModel>>> {

        private readonly ObservableCollection<IView> _views = new ObservableCollection<IView>();

        private readonly Dictionary<IView, ViewContainer> _viewContainers = new Dictionary<IView, ViewContainer>();

        private readonly FlowPanel _categoryFlowPanel = new FlowPanel();

        public ObservableCollection<IView> Views => _views;

        public int ViewHeight { get; set; } = 325;

        public ControlFlowDirection FlowDirection {
            get => _categoryFlowPanel.FlowDirection;
            set => _categoryFlowPanel.FlowDirection = value;
        }

        public RepeatedView() {
            _views.CollectionChanged += ViewsOnCollectionChanged;
        }

        private void ViewsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    HandleAddViews(e.NewItems.Cast<IView>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    HandleRemoveViews(e.OldItems.Cast<IView>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    // TODO: Implement replace collection changed
                    break;

                case NotifyCollectionChangedAction.Move:
                    // TODO: Implement move collection changed
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _categoryFlowPanel.ClearChildren();
                    _viewContainers.Clear();
                    break;
            }
        }

        private void ReflowViews() {
            int lastBottom = 0;

            foreach (var view in _views) {
                var viewContainter = _viewContainers[view];

                viewContainter.Location = new Point(0, lastBottom);

                lastBottom = viewContainter.Bottom;
            }
        }

        private void HandleAddViews(IEnumerable<IView> views) {
            foreach (var view in views) {
                var viewContainer = new ViewContainer() {
                    Size   = new Point(_categoryFlowPanel.Width, this.ViewHeight),
                    Parent = _categoryFlowPanel
                };

                viewContainer.Show(view);

                _viewContainers.Add(view, viewContainer);
            }

            ReflowViews();
        }

        private void HandleRemoveViews(IEnumerable<IView> views) {
            foreach (var view in views) {
                if (_viewContainers.ContainsKey(view)) {
                    _viewContainers[view].Dispose();
                    _viewContainers.Remove(view);
                }
            }

            ReflowViews();
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            _categoryFlowPanel.Size   = buildPanel.ContentRegion.Size;
            _categoryFlowPanel.Parent = buildPanel;
        }

    }
}
