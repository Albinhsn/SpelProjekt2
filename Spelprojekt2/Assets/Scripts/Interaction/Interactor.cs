using System;
using JetBrains.Annotations;
using srUtils.Unity;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Interaction
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private InstanceSet[] m_interactableSets;
        [SerializeField] private Transform m_targetOrigin;
        [SerializeField] private float m_range;
        [SerializeField] private float m_coneBaseRad;
        [SerializeField] private float m_coneFactor;

        [SerializeField] private LayerMask m_blockLineOfSight;

        [ItemCanBeNull] private Interactable[] m_selected;
        private int m_interacting = -1; //Interaction type. -1=not interacting

        public Vector3 aimDirection => m_targetOrigin.forward;

        public Vector3 position => m_targetOrigin.position;

        private void Awake()
        {
            m_selected = new Interactable?[m_interactableSets.Length];
        }

        private void Update()
        {
            switch (m_interacting)
            {
                case 0:
                    if(InputManager.Interact()) CancelInteractions();
                    break;
                case 1:
                    if(InputManager.PickedUpItem()) CancelInteractions();
                    break;
                default:
                    for (int a = 0; a < m_interactableSets.Length; a++)
                    {
                        SearchFrustum(a);
                    }
                    if (m_selected[0] is not null && InputManager.Interact())
                    {
                        Interact(m_selected[0], 0);
                    }
                    else if (m_selected[1] is not null && InputManager.PickedUpItem())
                    {
                        Interact(m_selected[1], 1);
                    }
                    break;
            }
        }

        public void Interact(Interactable interactable, int set_index)
        {
            if (interactable.requireUninteract)
            {
                m_interacting = set_index;
                interactable.SetHighlighted(false);
            }

            if (!interactable.canControlWhileInteracting)
            {
                InputManager.DisablePlayerInput();
            }

            m_selected[set_index] = interactable;
            interactable.Interact(this);
        }


        public void CancelInteractions()//Cancel interaction input
        {
            if(m_interacting == -1) return;
            
            if (m_selected[m_interacting].TryCancelInteraction())
            {
                InputManager.EnablePlayerInput();
           
                m_selected[m_interacting] = null;
                m_interacting = -1;
            }
        }

        public void FinishInteraction()//Interaction finished callback from interactable
        {
            Assert.IsTrue(m_interacting != -1, "FinishInteraction called on non-interacting interactor");
            InputManager.EnablePlayerInput();
            
            m_selected[m_interacting] = null;
            m_interacting = -1;
        }

        private void SearchFrustum(int set_index)
        {
            
            Interactable sel = null;
            float sel_distance = float.MaxValue;
            
            foreach (Interactable obj in m_interactableSets[set_index].GetEnumerable())
            {
                obj.m_indicator = IndicatorKind.None;
                Vector3 obj_relative_position = obj.position - m_targetOrigin.transform.position;
                float obj_linear_distance = Vector3.Dot(obj_relative_position, aimDirection);
                
                if(obj_linear_distance < 0 || obj_linear_distance > m_range) continue; //Out of range

                float obj_radial_distance = (obj_relative_position - aimDirection * obj_linear_distance).magnitude;
                if (obj_radial_distance > obj_linear_distance * m_coneFactor + m_coneBaseRad) continue; //Out of range
                
                obj.m_indicator = IndicatorKind.Target;

                if (obj_radial_distance < sel_distance && !Physics.Raycast(m_targetOrigin.position, obj_relative_position.normalized, obj_linear_distance, m_blockLineOfSight, QueryTriggerInteraction.Ignore))
                {
                    if(sel != null)
                    {
                        sel.m_indicator = IndicatorKind.Target;
                    }
                    obj.m_indicator = IndicatorKind.ClosestTarget;
                    sel = obj;
                    sel_distance = obj_radial_distance;
                }
            }

            if (m_selected[set_index] is not null && sel != m_selected[set_index])
            {
                m_selected[set_index].SetHighlighted(false);
            }
            m_selected[set_index] = sel;
            m_selected[set_index]?.SetHighlighted(true);

            foreach (Interactable obj in m_interactableSets[set_index].GetEnumerable())
            {
                obj.SendIndicatorRequest();
            }
        }
        

        private void OnDrawGizmos()
        {
            const int CONE_VERT_COUNT = 16;
            Gizmos.color = Color.yellow;
            Vector3[] cone_verts = new Vector3[CONE_VERT_COUNT * 4 + 8];
            
            for (int a = 0; a < CONE_VERT_COUNT * 2; a+=2)//Base
            {
                float v = (float)a / 2 / CONE_VERT_COUNT * 6.28318548f;//MathF.Tau apparently doesn't exist in this version
                cone_verts[a] = (new Vector3(
                    m_coneBaseRad * MathF.Cos(v),
                    m_coneBaseRad * MathF.Sin(v),
                    0
                    ));
                v = (float)(a / 2 + 1) / CONE_VERT_COUNT * 6.28318548f;
                cone_verts[a + 1] = (new Vector3(
                    m_coneBaseRad * MathF.Cos(v),
                    m_coneBaseRad * MathF.Sin(v),
                    0
                ));
            }

            float top_rad = m_coneBaseRad + m_coneFactor * m_range;
            for (int a = 0; a < CONE_VERT_COUNT * 2; a+=2)//Top
            {
                float v = (float)a / 2 / CONE_VERT_COUNT * 6.28318548f;
                cone_verts[a + CONE_VERT_COUNT * 2] = (new Vector3(
                    top_rad * MathF.Cos(v),
                    top_rad * MathF.Sin(v),
                    m_range
                ));
                v = (float)(a / 2 + 1) / CONE_VERT_COUNT * 6.28318548f;
                cone_verts[a + 1 + CONE_VERT_COUNT * 2] = (new Vector3(
                    top_rad * MathF.Cos(v),
                    top_rad * MathF.Sin(v),
                    m_range
                ));
            }
            
            
            cone_verts[CONE_VERT_COUNT * 4] = (new Vector3(m_coneBaseRad, 0, 0));
            cone_verts[CONE_VERT_COUNT * 4 + 2] = (new Vector3(0, m_coneBaseRad, 0));
            cone_verts[CONE_VERT_COUNT * 4 + 4] = (new Vector3(-m_coneBaseRad, 0, 0));
            cone_verts[CONE_VERT_COUNT * 4 + 6] = (new Vector3(0, -m_coneBaseRad, 0));
            cone_verts[CONE_VERT_COUNT * 4 + 1] = (new Vector3(top_rad, 0, m_range));
            cone_verts[CONE_VERT_COUNT * 4 + 3] = (new Vector3(0, top_rad, m_range));
            cone_verts[CONE_VERT_COUNT * 4 + 5] = (new Vector3(-top_rad, 0, m_range));
            cone_verts[CONE_VERT_COUNT * 4 + 7] = (new Vector3(0, -top_rad, m_range));

            //Transform
            Matrix4x4 visual_transform = Matrix4x4.Translate(m_targetOrigin.position);
            visual_transform *= Matrix4x4.Rotate(transform.rotation);
            for (int a = 0; a < CONE_VERT_COUNT*4+8; a++)
            {
                cone_verts[a] = visual_transform.MultiplyPoint(cone_verts[a]);
            }
            
            Gizmos.DrawLineList(cone_verts);
            
            Gizmos.DrawLine(position, position + aimDirection * m_range);
        }
    }
}
