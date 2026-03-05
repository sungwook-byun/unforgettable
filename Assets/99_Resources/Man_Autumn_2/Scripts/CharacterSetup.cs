using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NicePictures
{
    public class CharacterSetup : MonoBehaviour
    {       
        
        //list of all animations in the project for easy management
        public enum Animations
        {
            Idle, WalkInPlace, Walking, Running, Jumping, Falling, Landing
        }
        // Current animation for selected character prefab
        [SerializeField] private Animations _animations;
        private Animator _animator;
        // Start is called before the first frame update
        void Start()
        {
            _animator = GetComponent<Animator>();
            // _bodyMesh = GameObject.Find("Casual_9_TShirt");
        }

        // Update is called once per frame
        void Update()
        {
            if (_animator)
            {
                switch (_animations)
                {
                    case Animations.Idle:
                        _animator.SetBool("Idle", true);
                        break;
                    case Animations.WalkInPlace:
                        _animator.SetBool("WalkInPlace", true);
                        break;
                    case Animations.Walking:
                        _animator.SetBool("Walking", true);
                        break;
                    case Animations.Running:
                        _animator.SetBool("Running", true);
                        break;
                    case Animations.Jumping:
                        _animator.SetBool("Jumping", true);
                        break;
                    case Animations.Falling:
                        _animator.SetBool("Falling", true);
                        break;
                    case Animations.Landing:
                        _animator.SetBool("Landing", true);
                        break;
                }

            }
        }
    }
}