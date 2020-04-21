using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Xiv {
    public class QuestRewards {
        #region Fields
        private readonly Quest _Quest;
        private QuestRewardItemGroup[] _Items;
        #endregion

        #region Properties
        public Quest Quest { get { return _Quest; } }
        public ClassJob ClassJob { get { return Quest.As<ClassJob>("ClassJob{Unlock}"); } }
        public int Gil { get { return Quest.AsInt32("GilReward"); } }
        public int ExpFactor { get { return Quest.AsInt32("ExpFactor"); } }
        public int GrandCompanySeals { get { return Quest.AsInt32("GCSeals"); } }
        public IEnumerable<QuestRewardItemGroup> Items { get { return _Items ?? (_Items = BuildItems()); } }
        public Emote Emote { get { return Quest.As<Emote>("Emote{Reward}"); } }
        public Action Action { get { return Quest.As<Action>("Action{Reward}"); } }
        public GeneralAction GeneralAction { get { return Quest.As<GeneralAction>("GeneralAction{Reward}"); } }
        public InstanceContent InstanceContent { get { return Quest.As<InstanceContent>("InstanceContent{Unlock}"); } }
        public int Reputation { get { return Quest.AsInt32("ReputationReward"); } }
        public QuestRewardOther QuestRewardOther { get { return Quest.As<QuestRewardOther>("Other{Reward}"); } }
        #endregion

        #region Constructors
        public QuestRewards(Quest quest) {
            _Quest = quest;
        }
        #endregion

        #region Build
        private QuestRewardItemGroup[] BuildItems() {
            const int CatalystCount = 3;
            const int Group1Count = 6;
            const int Group2Count = 5;

            int groupsType = Quest.AsInt32("ItemRewardType");
            QuestRewardGroupType t1 = QuestRewardGroupType.Unknown;
            QuestRewardGroupType t2 = QuestRewardGroupType.Unknown;
            switch (groupsType) {
                case 0:
                    return new QuestRewardItemGroup[0];
                case 1:
                    t1 = QuestRewardGroupType.All;
                    t2 = QuestRewardGroupType.One;
                    break;
                case 3: // Gender-specific rewards.
                case 7: // Beast rank bonuses.
                    // Special handler
                    break;
                case 5:
                    t1 = QuestRewardGroupType.OnePerDay;
                    t2 = QuestRewardGroupType.OnePerDay;
                    break;
                case 6:
                    // Relic quests
                    break;
            }

            List<QuestRewardItemGroup> groups = new List<QuestRewardItemGroup>();

            QuestRewardItemGroup catalysts = BuildItemGroup(QuestRewardGroupType.All, "Item{Catalyst}", "ItemCount{Catalyst}", null, null, CatalystCount);
            groups.Add(catalysts);

            int tomestoneCount = Quest.AsInt32("TomestoneCount{Reward}");
            if (tomestoneCount > 0) {
                Item tomestoneItem = Quest.As<Item>("Tomestone{Reward}");
                if (tomestoneItem != null)
                {
                    groups.Add(
                        new QuestRewardItemGroup(
                            new[] { new QuestRewardItem(tomestoneItem, tomestoneCount, null, false) },
                            QuestRewardGroupType.All, null));
                }
            }

            if (groupsType == 3) {
                {
                    Item mItem = Quest.As<Item>("Item{Reward}[0]", 0);
                    int mCount = Quest.AsInt32("ItemCount{Reward}[0]", 0);
                    Stain mStain = Quest.As<Stain>("Stain{Reward}[0]", 0);

                    groups.Add(
                        new QuestRewardItemGroup(
                            new[] { new QuestRewardItem(mItem, mCount, mStain, false) },
                            QuestRewardGroupType.GenderSpecificMale, null));
                }
                {
                    Item fItem = Quest.As<Item>("Item{Reward}[0]", 1);
                    int fCount = Quest.AsInt32("ItemCount{Reward}[0]", 1);
                    Stain fStain = Quest.As<Stain>("Stain{Reward}[0]", 1);

                    groups.Add(
                        new QuestRewardItemGroup(
                            new[] { new QuestRewardItem(fItem, fCount, fStain, false) },
                            QuestRewardGroupType.GenderSpecificFemale, null));
                }
            }
            else if (groupsType == 6) {
                groups.AddRange(BuildClassQuestJobRewardItemGroups("Item{Reward}[0]", Group1Count));
                groups.Add(BuildItemGroup(t2, "Item{Reward}[1]", "ItemCount{Reward}[1]", "Stain{Reward}[1]", "IsHQ{Reward}[1]", Group2Count));
            }
            else if (groupsType == 7) {
                XivRow beastRankBonus = (XivRow)Quest.BeastTribe["BeastRankBonus"];
                Item item = beastRankBonus.As<Item>();
                List<int> counts = new List<int>();
                for (int i = 0; i < 8; i++)
                    counts.Add(beastRankBonus.AsInt32("Item{Quantity}", i));
                groups.Add(new QuestRewardItemGroup(new[] { new QuestRewardItem(item, counts.Distinct(), null, false) }, QuestRewardGroupType.BeastRankBonus, null));
            } else {
                groups.Add(BuildItemGroup(t1, "Item{Reward}[0]", "ItemCount{Reward}[0]", "Stain{Reward}[0]", null, Group1Count));
                groups.Add(BuildItemGroup(t2, "Item{Reward}[1]", "ItemCount{Reward}[1]", "Stain{Reward}[1]", "IsHQ{Reward}[1]", Group2Count));
            }

            return groups.Where(g => g.Items.Any()).ToArray();
        }
        private QuestRewardItemGroup BuildItemGroup(QuestRewardGroupType type, string itemPrefix, string countPrefix, string stainPrefix, string hqPrefix, int count) {
            List<QuestRewardItem> items = new List<QuestRewardItem>();

            for (int i = 0; i < count; ++i) {
                Item itm = Quest.As<Item>(itemPrefix, i);
                int c = Quest.AsInt32(countPrefix, i);

                if (itm == null || itm.Key == 0 || c == 0)
                    continue;

                Stain s = null;
                if (stainPrefix != null)
                    s = Quest.As<Stain>(stainPrefix, i);

                bool isHq = false;
                if (hqPrefix != null)
                    isHq = Quest.AsBoolean(hqPrefix, i);

                items.Add(new QuestRewardItem(itm, c, s, isHq));
            }

            return new QuestRewardItemGroup(items, type, null);
        }

        private List<QuestRewardItemGroup> BuildClassQuestJobRewardItemGroups(string itemPrefix, int count) {
            List<QuestRewardItemGroup> groups = new List<QuestRewardItemGroup>();
            
            for (int i = 0; i < count; ++i) {
                XivRow row = (XivRow)Quest[$"{itemPrefix}[{i}]"];
                if (row == null)
                    continue;

                Ex.Variant2.DataRow parentRow = (SaintCoinach.Ex.Variant2.DataRow)row.SourceRow;
                foreach (Ex.Variant2.SubRow subRow in parentRow.SubRows) {
                    ClassJobCategory category = (ClassJobCategory)subRow["ClassJobCategory"];
                    List<QuestRewardItem> items = new List<QuestRewardItem>();
                    for (int ii = 0; ii < 3; ++ii) {
                        Item itm = (Item)subRow["Reward{Item}[" + ii + "]"];
                        byte c = (byte)subRow["Reward{Amount}[" + ii + "]"];
                        if (itm.Key == 0 || c == 0 || category.Key == 0)
                            continue;

                        items.Add(new QuestRewardItem(itm, c, null, false));
                    }
                    groups.Add(new QuestRewardItemGroup(items, QuestRewardGroupType.ClassJob, category));
                }
            }

            return groups;
        }
        #endregion
    }
}
