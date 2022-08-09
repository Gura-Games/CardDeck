﻿using GuraGames.GameSystem;
using Pathfinding;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TomGustin.GameDesignPattern;
using UnityEngine;
using UnityEngine.Events;

namespace GuraGames.Character
{
    public class AIAgent : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private bool limitNodeDetect;
        [SerializeField, ShowIf("limitNodeDetect")] private int farestNodeDetect;

        //Pathfinding props
        protected AstarPath aStar;
        protected Seeker seeker;
        protected Path currentPath;

        private GraphNode lastNode;

        public void Init()
        {
            aStar = ServiceLocator.Resolve<AstarPath>();
            seeker = GetComponent<Seeker>();
        }

        public void CheckConjunctionNode(out (bool top, bool right, bool bottom, bool left) adjacent)
        {
            adjacent.top = adjacent.right = adjacent.bottom = adjacent.left = false;

            var referenceCheck = aStar.GetNearest(transform.position).node.Graph;
            Vector3 checkingPosition;

            checkingPosition = transform.position + GetNodeDirection(Vector2.up, referenceCheck);
            var topNode = aStar.GetNearest(checkingPosition).node;
            if (topNode != null)
            {
                var topCheck = topNode.Graph;
                adjacent.top = topCheck != null && !referenceCheck.Equals(topCheck);

                if (adjacent.top) adjacent.top = topNode.Walkable;
            }

            checkingPosition = transform.position + GetNodeDirection(Vector2.right, referenceCheck);
            var rightNode = aStar.GetNearest(checkingPosition).node;
            if (rightNode != null)
            {
                var rightCheck = rightNode.Graph;
                adjacent.right = rightCheck != null && !referenceCheck.Equals(rightCheck);

                if (adjacent.right) adjacent.right = rightNode.Walkable;
            }

            checkingPosition = transform.position + GetNodeDirection(Vector2.down, referenceCheck);
            var bottomNode = aStar.GetNearest(checkingPosition).node;
            if (bottomNode != null)
            {
                var bottomCheck = bottomNode.Graph;
                adjacent.bottom = bottomCheck != null && !referenceCheck.Equals(bottomCheck);

                if (adjacent.bottom) adjacent.bottom = bottomNode.Walkable;
            }

            checkingPosition = transform.position + GetNodeDirection(Vector2.left, referenceCheck);
            var leftNode = aStar.GetNearest(checkingPosition).node;
            if (leftNode != null)
            {
                var leftCheck = leftNode.Graph;
                adjacent.left = leftCheck != null && !referenceCheck.Equals(leftCheck);

                if (adjacent.left) adjacent.left = leftNode.Walkable;
            }
        }

        public IEnumerator Scan(Vector3 target_position, UnityAction onCompleteScan, bool forceScan = false)
        {
            var nearest = aStar.GetNearest(target_position);

            if (!forceScan && (lastNode != null && lastNode.Equals(nearest.node))) yield break;
            lastNode = nearest.node;

            if (lastNode.Walkable)
            {
                GGDebug.Console("Move to: " + (Vector3)lastNode.position);
                currentPath = seeker.StartPath(transform.position, (Vector3)lastNode.position);

                yield return new WaitUntil(currentPath.IsDone);
                if (limitNodeDetect && currentPath.path.Count > farestNodeDetect + 1)
                {
                    GGDebug.Console("Outside Max Node Move", Enums.DebugType.Warning);
                    yield break;
                }

                GGDebug.Console("Current path " + currentPath.path.Count);

                onCompleteScan?.Invoke();
            }
            else
            {
                GGDebug.Console("Blocked", Enums.DebugType.Warning);
            }
        }

        public Path GetScannedPath()
        {
            return currentPath;
        }

        public float GetCurrentNodeSize()
        {
            var graph = GetCurrentActiveGraph();

            if (graph == null) return -1;

            return ((GridGraph) graph).nodeSize;
        }

        public GridGraph GetCurrentActiveGraph()
        {
            return aStar.GetNearest(transform.position).node.Graph as GridGraph;
        }

        public GridGraph GetGraphOn(Vector2 position)
        {
            return aStar.GetNearest(position).node.Graph as GridGraph;
        }

        public Vector3 GetNodePositionOn(Vector2 position)
        {
            return (Vector3) aStar.GetNearest(position).node.position;
        }

        private float GetNodeSize(NavGraph graph)
        {
            return ((GridGraph)graph).nodeSize;
        }

        private Vector3 GetNodeDirection(Vector2 direction, NavGraph graph)
        {
            return direction * GetNodeSize(graph);
        }
    }
}