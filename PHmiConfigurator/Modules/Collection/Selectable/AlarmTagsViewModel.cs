﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using PHmiClient.Converters;
using PHmiConfigurator.Dialogs;
using PHmiModel.Entities;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection.Selectable {
    public class
        AlarmTagsViewModel : SelectableCollectionViewModel<AlarmTag, AlarmTag.AlarmTagMetadata, AlarmCategory
        > {
        public AlarmTagsViewModel() : base(null) { }

        public override string Name {
            get { return Res.AlarmTags; }
        }

        protected override void PostReloadAction() {
            var digitalTags = Context.Get<DigTag>().OrderBy(t => t.IoDevice.Name).ThenBy(t => t.Name)
                .ToArray();
            DigitalTags = digitalTags;
            _digitalTagsDictionary = new Dictionary<string, DigTag>(digitalTags.Length);
            foreach (DigTag tag in digitalTags) {
                string key = tag.IoDevice.Name + "." + tag.Name;
                if (!_digitalTagsDictionary.ContainsKey(key))
                    _digitalTagsDictionary.Add(key, tag);
            }

            base.PostReloadAction();
        }

        protected override IEditDialog<AlarmTag.AlarmTagMetadata> CreateAddDialog() {
            return new EditAlarmTag {
                Title = Res.AddAlarmTag,
                Owner = Window.GetWindow(View),
                DigitalTags = DigitalTags
            };
        }

        protected override IEditDialog<AlarmTag.AlarmTagMetadata> CreateEditDialog() {
            return new EditAlarmTag {
                Title = Res.EditAlarmTag,
                Owner = Window.GetWindow(View),
                DigitalTags = DigitalTags
            };
        }

        protected override string[] GetCopyData(AlarmTag item) {
            return new[] {
                item.DigTag.IoDevice.Name,
                item.DigTag.Name,
                item.Location,
                item.Description,
                item.Acknowledgeable.ToString(CultureInfo.InvariantCulture),
                Int32ToPrivilegedConverter.Convert(item.Privilege)
            };
        }

        protected override string[] GetCopyHeaders() {
            return new[] {
                Res.IoDevice,
                Res.DigitalTag,
                Res.Location,
                Res.Description,
                Res.Acknowledgeable,
                Res.Privilege
            };
        }

        protected override void SetCopyData(AlarmTag item, string[] data) {
            item.DigTag = _digitalTagsDictionary[data[0] + "." + data[1]];
            item.Location = data[2];
            item.Description = data[3];
            item.Acknowledgeable = bool.Parse(data[4]);
            item.Privilege = Int32ToPrivilegedConverter.ConvertBack(data[5]);
        }

        #region DigitalTags

        private Dictionary<string, DigTag> _digitalTagsDictionary;

        private IEnumerable<DigTag> _digitalTags;

        public IEnumerable<DigTag> DigitalTags {
            get { return _digitalTags; }
            set {
                _digitalTags = value;
                OnPropertyChanged(this, v => v.DigitalTags);
            }
        }

        #endregion DigitalTags
    }
}