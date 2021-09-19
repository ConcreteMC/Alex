using System;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;

namespace Alex.Graphics.Models.Entity.Animations
{
	//TODO: Get rid of all the string lookups
	
	/// <summary>
	///		
	/// </summary>
	public class AnimationController : IAnimation
	{
		public AnimationComponent Parent { get; }
		public ResourcePackLib.Json.Bedrock.Entity.AnimationController Definition { get; }

		private AnimationState _state;
		public AnimationController(AnimationComponent parent,
			ResourcePackLib.Json.Bedrock.Entity.AnimationController definition)
		{
			Parent = parent;
			Definition = definition;


			if (string.IsNullOrWhiteSpace(definition.InitialState)
			         || !definition.States.TryGetValue(definition.InitialState, out _state))
			{
				_state = definition.States.FirstOrDefault().Value;
			}
			
			if (_state == null)
				throw new Exception("Initial state not found!");
		}

		public void Tick()
		{
			var state = _state;

			if (state == null)
				return;

			ExecuteState(state);
		}

		private void ExecuteState(AnimationState state)
		{
			UpdateVariables(state.Variables);
			UpdateAnimations(state.Animations);
			UpdateTransitions(state.Transitions);
		}

		/// <inheritdoc />
		public void UpdateBindings(ModelRenderer renderer)
		{
			
		}

		private void UpdateTransitions(AnnoyingMolangElement[] transitions)
		{
			if (transitions == null || transitions.Length == 0)
				return;
			
			bool stateUpdated = false;
			foreach (var transition in transitions)
			{
				foreach (var expression in transition.Expressions)
				{
					//TODO: Pre-verify if the states exist on initialization of the animation controller
					if (!Definition.States.TryGetValue(expression.Key, out var targetState) 
					    || targetState == null) 
						continue;
						
					var result = Parent.Execute(expression.Value);

					if (result.AsBool())
					{
						var previousState = _state;

						if (previousState != null)
						{
							UpdateAnimations(previousState.Animations, true);
						}
						_state = targetState;
						stateUpdated = true;
						
						
						break;
					}
				}

				if (stateUpdated)
					break;
			}
		}

		private void UpdateAnimations(AnnoyingMolangElement[] animations, bool forceStop = false)
		{
			if (animations == null || animations.Length == 0)
				return;
			
			foreach (var animation in animations)
			{
				Parent.ExecuteAnnoying(animation, forceStop);
			}
		}

		private void UpdateVariables(IDictionary<string, ControllerVariable> variables)
		{
			if (variables == null || variables.Count == 0)
				return;
			
			foreach (var anim in variables)
			{
				if (anim.Value.Input != null)
				{
					var input = Parent.Execute(anim.Value.Input);//.AsDouble();
					Parent.Runtime.Environment.Structs["variable"].Set(anim.Key, input);
				}
				//HandleAnnoyingMolangElement(runtime, anim, context);
			}
		}
	}
}