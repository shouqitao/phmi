﻿using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel.Entities;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection {
    public class
        AlarmCategoriesViewModel : CollectionViewModel<AlarmCategory, AlarmCategory.AlarmCategoryMetadata> {
        public AlarmCategoriesViewModel() : base(null) { }

        public override string Name {
            get { return Res.AlarmCategories; }
        }

        protected override IEditDialog<AlarmCategory.AlarmCategoryMetadata> CreateAddDialog() {
            return new EditAlarmCategory {Title = Res.AddAlarmCategory, Owner = Window.GetWindow(View)};
        }

        protected override IEditDialog<AlarmCategory.AlarmCategoryMetadata> CreateEditDialog() {
            return new EditAlarmCategory {Title = Res.EditAlarmCategory, Owner = Window.GetWindow(View)};
        }

        protected override string[] GetCopyData(AlarmCategory item) {
            return new[] {
                item.Description,
                item.TimeToStore
            };
        }

        protected override string[] GetCopyHeaders() {
            return new[] {
                ReflectionHelper.GetDisplayName<AlarmCategory>(a => a.Description),
                ReflectionHelper.GetDisplayName<AlarmCategory>(a => a.TimeToStore)
            };
        }

        protected override void SetCopyData(AlarmCategory item, string[] data) {
            item.Description = data[0];
            item.TimeToStore = data[1];
        }
    }
}