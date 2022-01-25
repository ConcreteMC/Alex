using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Alex.Common.Graphics;
using Alex.Common.World;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models;

public class ModelMatrixHolder : IDisposable
{
	private IModel _model;

	public virtual IModel Model
	{
		get
		{
			return _model;
		}
		set
		{
			var oldValue = _model;
			_model = value;
			OnModelChanged(oldValue, value);
		}
	}

	private ConcurrentDictionary<string, BoneMatrices> _boneTransforms =
		new ConcurrentDictionary<string, BoneMatrices>(StringComparer.InvariantCultureIgnoreCase);

	public ModelMatrixHolder() { }

	protected internal void OnModelChanged(IModel oldValue, IModel model)
	{
		if (oldValue?.Bones != null)
		{
			oldValue.Bones.CollectionChanged -= CollectionChanged;
		}

		_boneTransforms.Clear();

		if (model != null)
		{
			model.Bones.CollectionChanged += CollectionChanged;

			var bones = model.Bones;

			if (bones != null)
			{
				foreach (var bone in bones)
				{
					_boneTransforms.TryAdd(bone.Name, new BoneMatrices(this));
				}
			}
		}
	}

	protected virtual void ModelChanged(Model newModel) { }

	private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems != null)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (ModelBone newBone in e.NewItems)
				{
					_boneTransforms.TryAdd(newBone.Name, new BoneMatrices(this));
				}
			}
		}

		if (e.OldItems != null)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (ModelBone bone in e.OldItems)
				{
					_boneTransforms.TryRemove(bone.Name, out _);
				}
			}
		}
	}

	public bool IsMatricesDirty { get; set; } = false;
	private Matrix[] _transforms = null;

	public Matrix[] GetTransforms()
	{
		var model = Model;

		if (model?.Bones == null)
			return null;

		if (_transforms != null && !IsMatricesDirty && _transforms.Length == model.Bones.Count)
			return _transforms;

		var source = model.Bones.ImmutableArray;
		Matrix[] destinationBoneTransforms = new Matrix[source.Length];

		var bones = source;

		if (destinationBoneTransforms == null)
			throw new ArgumentNullException(nameof(destinationBoneTransforms));

		//var bones = this.Bones;
		if (destinationBoneTransforms.Length < bones.Length)
			throw new ArgumentOutOfRangeException(nameof(destinationBoneTransforms));

		int count = bones.Length;

		for (int index1 = 0; index1 < count; index1++)
		{
			if (index1 >= bones.Length)
				break;

			var modelBone = bones[index1];

			if (GetBoneTransform(modelBone.Name, out var boneData))
			{
				if (modelBone.Parent == null)
				{
					destinationBoneTransforms[index1] = modelBone.GetTransform(boneData);
				}
				else
				{
					int parentIndex = modelBone.Parent.Index;

					destinationBoneTransforms[index1] = Matrix.Multiply(
						modelBone.GetTransform(boneData), destinationBoneTransforms[parentIndex]);
				}
			}
		}

		_transforms = destinationBoneTransforms;
		IsMatricesDirty = false;

		return destinationBoneTransforms;
	}

	public bool GetBoneTransform(string name, out BoneMatrices bone)
	{
		if (_boneTransforms.TryGetValue(name, out bone))
		{
			return true;
		}

		return false;
	}

	private BoneMatrices[] _boneMatrices = Array.Empty<BoneMatrices>();

	public virtual void Update(IUpdateArgs args)
	{
		var matrices = _boneMatrices;
		foreach (var bone in matrices)
		{
			bone.Update(args);
			//bone.Update(args);
		}
	}

	public virtual void ApplyMovement()
	{
		var matrices = _boneTransforms.Values.ToArray();
		foreach (var b in matrices)
		{
			b.ApplyMovement();
		}

		_boneMatrices = matrices;
	}

	/// <inheritdoc />
	public virtual void Dispose()
	{
		_model?.Dispose();
	}
}