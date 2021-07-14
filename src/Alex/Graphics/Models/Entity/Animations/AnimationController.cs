using System;
using System.Collections.Generic;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;

namespace Alex.Graphics.Models.Entity.Animations
{
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

			if (!definition.States.TryGetValue(definition.InitialState, out _state))
			{
				throw new Exception("Initial state not found!");
			}
		}

		public void Update()
		{
			var state = _state;

			if (state == null)
				return;

			UpdateVariables(state.Variables);
			
			UpdateAnimations(state.Animations);

			UpdateTransitions(state.Transitions);
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
					if (!Definition.States.TryGetValue(expression.Key, out var targetState) 
					    || targetState == null) 
						continue;
						
					var result = Parent.Execute(expression.Value);

					if (result.AsBool())
					{
						_state = targetState;
						stateUpdated = true;
						break;
					}
				}

				if (stateUpdated)
					break;
			}
		}

		private void UpdateAnimation(string animation, bool play)
		{
			if (Parent.TryGetAnimation(animation, out var entityAnimation))
			{
				if (entityAnimation.Playing && !play)
				{
					entityAnimation.Stop();
				}
				else if (!entityAnimation.Playing && play)
				{
					entityAnimation.Play();
				}

				if (entityAnimation.Playing)
				{
					entityAnimation.BeforeUpdate();
					
					entityAnimation.Update();

					entityAnimation.AfterUpdate();
				}
			}
		}
		
		private void UpdateAnimations(AnnoyingMolangElement[] animations)
		{
			if (animations == null || animations.Length == 0)
				return;
			
			foreach (var animation in animations)
			{
				if (animation.IsString)
				{
					UpdateAnimation(animation.StringValue, true);
				}
				else
				{
					foreach (var expression in animation.Expressions)
					{
						var shouldPlay = Parent.Execute(expression.Value).AsBool();
						UpdateAnimation(expression.Key, shouldPlay);
					}
				}
				//Parent.Execute(animation);
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