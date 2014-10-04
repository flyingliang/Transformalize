using Transformalize.Libs.DBDiff.Schema.Model;
using Transformalize.Libs.DBDiff.Schema.SqlServer2005.Model;

namespace Transformalize.Libs.DBDiff.Schema.SqlServer2005.Compare
{
    internal class CompareTriggers:CompareBase<Trigger>
    {
        protected override void DoNew<Root>(SchemaList<Trigger, Root> CamposOrigen, Trigger node)
        {
            Trigger newNode = (Trigger)node.Clone(CamposOrigen.Parent);
            newNode.Status = Enums.ObjectStatusType.CreateStatus;
            CamposOrigen.Add(newNode);
        }

        protected override void DoUpdate<Root>(SchemaList<Trigger, Root> CamposOrigen, Trigger node)
        {
            if (!node.Compare(CamposOrigen[node.FullName]))
            {
                Trigger newNode = (Trigger)node.Clone(CamposOrigen.Parent);
                if (!newNode.Text.Equals(CamposOrigen[node.FullName].Text))
                    newNode.Status = Enums.ObjectStatusType.AlterStatus;
                if (node.IsDisabled != CamposOrigen[node.FullName].IsDisabled)
                    newNode.Status = newNode.Status + (int)Enums.ObjectStatusType.DisabledStatus;
                CamposOrigen[node.FullName] = newNode;
            }
        }
    }
}
