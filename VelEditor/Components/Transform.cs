using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using VelEditor.Utilities;

namespace VelEditor.Components
{
    [DataContract]
    class Transform : Component
    {
        private Vector3 _position;
        [DataMember]
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        private Vector3 _rotation;
        [DataMember]
        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    OnPropertyChanged(nameof(Rotation));
                }
            }
        }

        private Vector3 _scale;
        [DataMember]
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged(nameof(Scale));
                }
            }
        }
        public override IMSComponent GetMultiSelectionComponent(MSEntity msEntity) => new MSTransform(msEntity);

        public override void WriteToBinary(BinaryWriter bw)
        {
            bw.Write(_position.X); bw.Write(_position.Y); bw.Write(_position.Z);
            bw.Write(_rotation.X); bw.Write(_rotation.Y); bw.Write(_rotation.Z);
            bw.Write(_scale.X); bw.Write(_scale.Y); bw.Write(_scale.Z);
        }

        public Transform(GameEntity entity) : base(entity)
        {

        }

    }

    sealed class MSTransform : MSComponent<Transform>
    {
        private float? _positionX;

        public float? PositionX
        {
            get => _positionX;
            set
            {
                if (!_positionX.IsTheSameAs(value))
                {
                    _positionX = value;
                    OnPropertyChanged(nameof(PositionX));
                }
            }
        }

        private float? _positionY;
        public float? PositionY
        {
            get => _positionY;
            set
            {
                if (!_positionY.IsTheSameAs(value))
                {
                    _positionY = value;
                    OnPropertyChanged(nameof(PositionY));
                }
            }
        }

        private float? _positionZ;
        public float? PositionZ
        {
            get => _positionZ;
            set
            {
                if (!_positionZ.IsTheSameAs(value))
                {
                    _positionZ = value;
                    OnPropertyChanged(nameof(PositionZ));
                }
            }
        }

        private float? _rotationX;
        public float? RotationX
        {
            get => _rotationX;
            set
            {
                if (!_rotationX.IsTheSameAs(value))
                {
                    _rotationX = value;
                    OnPropertyChanged(nameof(RotationX));
                }
            }
        }
        

        private float? _rotationY;
        public float? RotationY
        {
            get => _rotationY;
            set
            {
                if (!_rotationY.IsTheSameAs(value))
                {
                    _rotationY = value;
                    OnPropertyChanged(nameof(RotationY));
                }
            }
        }

        private float? _rotationZ;
        public float? RotationZ
        {
            get => _rotationZ;
            set
            {
                if (!_rotationZ.IsTheSameAs(value))
                {
                    _rotationZ = value;
                    OnPropertyChanged(nameof(RotationZ));
                }
            }
        }

        private float? _scaleX;
        public float? ScaleX
        {
            get => _scaleX;
            set
            {
                if (!_scaleX.IsTheSameAs(value))
                {
                    _scaleX = value;
                    OnPropertyChanged(nameof(ScaleX));
                }
            }
        }

        private float? _scaleY;
        public float? ScaleY
        {
            get => _scaleY;
            set
            {
                if (!_scaleY.IsTheSameAs(value))
                {
                    _scaleY = value;
                    OnPropertyChanged(nameof(ScaleY));
                }
            }
        }
        

        private float? _scaleZ;
        public float? ScaleZ
        {
            get => _scaleZ;
            set
            {
                if (!_scaleZ.IsTheSameAs(value))
                {
                    _scaleZ = value;
                    OnPropertyChanged(nameof(ScaleZ));
                }
            }
        }


        public MSTransform(MSEntity msEntity) : base(msEntity)
        {
            Refresh();
        }

        protected override bool UpdateComponents(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(PositionX):
                case nameof(PositionY):
                case nameof(PositionZ):
                    SelectedComponents.ForEach(c => c.Position = new Vector3(
                        _positionX ?? c.Position.X, _positionY ?? c.Position.Y, _positionZ ?? c.Position.Z
                        ));
                    return true;
                case nameof(RotationX):
                case nameof(RotationY):
                case nameof(RotationZ):
                    SelectedComponents.ForEach(c => c.Rotation = new Vector3(
                        _rotationX ?? c.Rotation.X, _rotationY ?? c.Rotation.Y, _rotationZ ?? c.Rotation.Z
                        ));
                    return true;

                case nameof(ScaleX):
                case nameof(ScaleY):
                case nameof(ScaleZ):
                    SelectedComponents.ForEach(c => c.Scale = new Vector3(
                        _scaleX ?? c.Scale.X, _scaleY ?? c.Scale.Y, _scaleZ ?? c.Scale.Z
                        ));
                    return true;
            }
            return false;
        }

        protected override bool UpdateMSComponents()
        {
            PositionX = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Position.X));
            PositionY = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Position.Y));
            PositionZ = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Position.Z));

            RotationX = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Rotation.X));
            RotationY = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Rotation.Y));
            RotationZ = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Rotation.Z));

            ScaleX = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Scale.X));
            ScaleY = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Scale.Y));
            ScaleZ = MSEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(x => x.Scale.Z));

            return true;
        }
    }

}
