﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiClient.Utils;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiModel.Entities {
    [MetadataType(typeof(AlarmTagMetadata))]
    [Table("alarm_tags", Schema = "public")]
    public class AlarmTag : NamedEntity {
        public class AlarmTagMetadata : EntityMetadataBase {
            private bool _acknowledgeable;
            private string _description;
            private DigTag _digTag;
            private string _location;
            private string _name;
            private int? _privilege;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage",
                ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage",
                ErrorMessageResourceType = typeof(Res))]
            public string Name {
                get { return _name; }
                set {
                    _name = value;
                    OnPropertyChanged(this, m => m.Name);
                }
            }

            [LocDisplayName("DigitalTag", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage",
                ErrorMessageResourceType = typeof(Res))]
            public DigTag DigTag {
                get { return _digTag; }
                set {
                    _digTag = value;
                    OnPropertyChanged(this, m => m.DigTag);
                }
            }

            [LocDisplayName("Location", ResourceType = typeof(Res))]
            public string Location {
                get { return _location; }
                set {
                    _location = value;
                    OnPropertyChanged(this, m => m.Location);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            public string Description {
                get { return _description; }
                set {
                    _description = value;
                    OnPropertyChanged(this, m => m.Description);
                }
            }

            [LocDisplayName("Acknowledgeable", ResourceType = typeof(Res))]
            public bool Acknowledgeable {
                get { return _acknowledgeable; }
                set {
                    _acknowledgeable = value;
                    OnPropertyChanged(this, m => m.Acknowledgeable);
                }
            }

            [LocDisplayName("Privilege", ResourceType = typeof(Res))]
            public int? Privilege {
                get { return _privilege; }
                set {
                    _privilege = value;
                    OnPropertyChanged(this, m => m.Privilege);
                }
            }
        }

        #region RefDigTag

        private int _refDigTag;

        [Column("ref_dig_tags")]
        public int RefDigTag {
            get { return _refDigTag; }
            set {
                _refDigTag = value;
                OnPropertyChanged(this, t => t.DigTag);
                OnPropertyChanged(this, e => RefDigTag);
            }
        }

        #endregion RefDigTag

        #region Location

        private string _location;

        [Column("location")]
        public string Location {
            get { return _location; }
            set {
                _location = value;
                OnPropertyChanged(this, e => e.Location);
            }
        }

        #endregion Location

        #region Description

        private string _description;

        [Column("description")]
        public string Description {
            get { return _description; }
            set {
                _description = value;
                OnPropertyChanged(this, e => e.Description);
            }
        }

        #endregion Description

        #region Privilege

        private int? _privilege;

        [Column("privilege")]
        public int? Privilege {
            get { return _privilege; }
            set {
                _privilege = value;
                OnPropertyChanged(this, e => e.Privilege);
            }
        }

        #endregion Privilege

        #region RefCategories

        private int _refCategory;

        [Column("ref_categories")]
        public int RefCategory {
            get { return _refCategory; }
            set {
                _refCategory = value;
                OnPropertyChanged(this, e => e.RefCategory);
            }
        }

        #endregion RefCategories

        #region Acknowledgeable

        private bool _acknowledgeable;

        [Column("acknowledgeable")]
        public bool Acknowledgeable {
            get { return _acknowledgeable; }
            set {
                _acknowledgeable = value;
                OnPropertyChanged(this, e => e.Acknowledgeable);
            }
        }

        #endregion Acknowledgeable

        #region AlarmCategory

        private AlarmCategory _alarmCategory;

        public virtual AlarmCategory AlarmCategory {
            get { return _alarmCategory; }
            set {
                _alarmCategory = value;
                OnPropertyChanged(this, e => e.AlarmCategory);
            }
        }

        #endregion AlarmCategory

        #region DigTag

        private DigTag _digTag;

        public virtual DigTag DigTag {
            get { return _digTag; }
            set {
                _digTag = value;
                OnPropertyChanged(this, e => e.DigTag);
            }
        }

        #endregion DigTag
    }
}