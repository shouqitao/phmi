using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel.Entities;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection {
    public class
        TrendCategoriesViewModel : CollectionViewModel<TrendCategory, TrendCategory.TrendCategoryMetadata> {
        public TrendCategoriesViewModel() : base(null) { }

        public override string Name {
            get { return Res.TrendCategories; }
        }

        protected override IEditDialog<TrendCategory.TrendCategoryMetadata> CreateAddDialog() {
            return new EditTrendCategory {Title = Res.AddTrendCategory, Owner = Window.GetWindow(View)};
        }

        protected override IEditDialog<TrendCategory.TrendCategoryMetadata> CreateEditDialog() {
            return new EditTrendCategory {Title = Res.EditTrendCategory, Owner = Window.GetWindow(View)};
        }

        protected override string[] GetCopyData(TrendCategory item) {
            return new[] {
                item.TimeToStore
            };
        }

        protected override string[] GetCopyHeaders() {
            return new[] {
                ReflectionHelper.GetDisplayName<TrendCategory>(a => a.TimeToStore)
            };
        }

        protected override void SetCopyData(TrendCategory item, string[] data) {
            item.TimeToStore = data[0];
        }
    }
}