using System.Collections;
using System.Linq;
using Transformalize.Configuration;

namespace Transformalize.Main {

    public class OperationsLoader {
        private const string DEFAULT = "[default]";
        private readonly Process _process;
        private readonly EntityElementCollection _entities;

        public OperationsLoader(ref Process process, EntityElementCollection entities) {
            _process = process;
            _entities = entities;
        }

        public void Load() {
            foreach (EntityConfigurationElement entityElement in _entities) {

                var entity = _process.Entities.First(e => e.Alias == entityElement.Alias);
                var factory = new TransformOperationFactory(_process);

                //fields can have prefixes and are limited to literal parameters (parameters with name and value provided in configuration)
                foreach (FieldConfigurationElement f in entityElement.Fields) {

                    var alias = Common.GetAlias(f, true, entityElement.Prefix);
                    var field = _process.GetField(alias, entity.Alias);

                    foreach (TransformConfigurationElement t in f.Transforms) {
                        var reader = new FieldParametersReader();
                        entity.Operations.Add(factory.Create(field, t, reader.Read(t)));
                        AddBranches(t.Branches, entity, field, reader);
                    }
                }

                // calculated fields do not have prefixes, and have access to all or some of an entity's parameters
                foreach (FieldConfigurationElement cf in entityElement.CalculatedFields) {

                    var field = _process.GetField(cf.Alias, entity.Alias);

                    foreach (TransformConfigurationElement t in cf.Transforms) {
                        var reader = t.Parameter.Equals("*") ?
                            (ITransformParametersReader)new EntityParametersReader(entity) :
                            new EntityTransformParametersReader(entity);
                        entity.Operations.Add(factory.Create(field, t, reader.Read(t)));
                        AddBranches(t.Branches, entity, field, reader);
                    }
                }
            }
        }

        private void AddBranches(IEnumerable branches, Entity entity, Field field, ITransformParametersReader reader) {
            foreach (BranchConfigurationElement branch in branches) {
                foreach (TransformConfigurationElement transform in branch.Transforms) {

                    transform.RunField = branch.RunField.Equals(DEFAULT) ? (Common.IsValidator(transform.Method) ? (transform.ResultField.Equals(DEFAULT) ? transform.ResultField + "Result" : transform.ResultField) : field.Alias) : branch.RunField;
                    transform.RunOperator = branch.RunOperator;
                    transform.RunValue = branch.RunValue;

                    var operation = new TransformOperationFactory(_process).Create(field, transform, reader.Read(transform));
                    entity.Operations.Add(operation);
                    if (transform.Branches.Count > 0) {
                        AddBranches(transform.Branches, entity, field, reader);
                    }
                }
            }
        }

    }
}