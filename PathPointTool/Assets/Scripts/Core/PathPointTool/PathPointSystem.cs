﻿/*
 * Description:             PathPointSystem.cs
 * Author:                  TONYTANG
 * Create Date:             2023/04/09
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathPoint
{
    /// <summary>
    /// PathPointSystem.cs
    /// 路点系统组件
    /// </summary>
    public class PathPointSystem : MonoBehaviour
    {
        /// <summary>
        /// 路点父节点名
        /// </summary>
        private const string PathPointParentNodeName = "Path";

        /// <summary>
        /// 路线类型
        /// </summary>
        [Header("路线类型")]
        public PathType Type = PathType.Normal;

        /// <summary>
        /// 路线绘制类型
        /// </summary>
        [Header("路线绘制类型")]
        public PathDrawType DrawType = PathDrawType.Line;

        /// <summary>
        /// 路点起始位置
        /// </summary>
        [Header("路点起始位置")]
        public Vector3 PathPointStartPos = Vector3.zero;

        /// <summary>
        /// 路点间隔
        /// </summary>
        [Header("路点间隔")]
        public float PathPointGap = 1;

        /// <summary>
        /// 绘制路点间隔距离
        /// </summary>
        [Header("绘制路点间隔距离")]
        public float DrawPathPointDistance = 0.2f;

        /// <summary>
        /// 路点球体大小
        /// </summary>
        [Header("路点球体大小")]
        [Range(1f, 10f)]
        public float PathPointSphereSize = 1f;

        /// <summary>
        /// 路点球体颜色
        /// </summary>
        [Header("路点球体颜色")]
        public Color PathPointSphereColor = Color.green;

        /// <summary>
        /// 路线绘制颜色
        /// </summary>
        [Header("路线绘制颜色")]
        public Color PathDrawColor = Color.yellow;

        /// <summary>
        /// 细分路点绘制颜色
        /// </summary>
        [Header("细分路点绘制颜色")]
        public Color PathPointDrawColor = Color.blue;

        /// <summary>
        /// 路点对象列表
        /// </summary>
        [Header("路点对象列表")]
        public List<Transform> PathPointList = new List<Transform>();

        /// <summary>
        /// 路点父节点
        /// </summary>
        [Header("路点父节点")]
        public Transform PathPointParentNode;

        private void Awake()
        {
            InitPathPointParentNode();
            CheckPathPointParentNode();
        }

        private void Update()
        {
            CheckPathPointParentNode();
            CheckPathPointNodes();
        }

        /// <summary>
        /// 初始化路点父节点
        /// </summary>
        private void InitPathPointParentNode()
        {
            if (PathPointParentNode == null)
            {
                PathPointParentNode = new GameObject(PathPointParentNodeName).transform;
                PathPointParentNode.SetParent(transform);
                PathPointParentNode.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// 路点父节点检查
        /// </summary>
        private void CheckPathPointParentNode()
        {
            if (PathPointParentNode == null)
            {
                InitPathPointParentNode();
            }
            if (PathPointParentNode != null && PathPointParentNode.parent != transform)
            {
                PathPointParentNode.SetParent(transform);
                PathPointParentNode.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// 检查路点子节点
        /// </summary>
        private void CheckPathPointNodes()
        {
            for(int i = PathPointList.Count - 1; i  >= 0; i--)
            {
                if(PathPointList[i] == null)
                {
                    RemovePathPointByIndex(i);
                }
            }
        }

        /// <summary>
        /// 指定位置索引添加路点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool AddPathPointByIndex(int index)
        {
            var pathPointNum = PathPointList.Count;
            if (index < 0 || index > pathPointNum)
            {
                Debug.LogError($"指定索引:{index}不是有效索引范围:{0}-{pathPointNum}，添加路点失败！");
                return false;
            }
            var newPathPoint = ConstructNewPathPoint(index);
            PathPointList.Insert(index, newPathPoint);
            OnPathPointNumChanged();
            return true;
        }

        /// <summary>
        /// 移除指定索引的路点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool RemovePathPointByIndex(int index)
        {
            var pathPointNum = PathPointList.Count;
            if (index < 0 || index >= pathPointNum)
            {
                Debug.LogError($"指定索引:{index}不是有效索引范围:{0}-{pathPointNum - 1}，移除路点失败！");
                return false;
            }
            DestroyPathPointByIndex(index);
            PathPointList.RemoveAt(index);
            OnPathPointNumChanged();
            return true;
        }

        /// <summary>
        /// 销毁指定索引路点对象
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool DestroyPathPointByIndex(int index)
        {
            var pathPointNum = PathPointList.Count;
            if (index < 0 || index >= pathPointNum)
            {
                Debug.LogError($"指定索引:{index}不是有效索引范围:{0}-{pathPointNum - 1}，销毁路点对象失败！");
                return false;
            }
            if(PathPointList[index] != null)
            {
                GameObject.DestroyImmediate(PathPointList[index].gameObject);
            }
            return true;
        }

        /// <summary>
        /// 响应路点数量变化
        /// </summary>
        private void OnPathPointNumChanged()
        {
            UpdatePathPointNames();
        }

        /// <summary>
        /// 更新路点节点名字
        /// </summary>
        public void UpdatePathPointNames()
        {
            for(int i = 0, length = PathPointList.Count; i < length; i++)
            {
                var pathPoint = PathPointList[i];
                if (pathPoint != null)
                {
                    pathPoint.name = GetPathPointNameByIndex(i);
                }
            }
        }

        /// <summary>
        /// 构建一个新的路点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Transform ConstructNewPathPoint(int index)
        {
            // 路点初始位置根据路点索引是否有有效路点决定
            // 没有任何路点则用路点起始位置
            // 有有效路点则用当前构建位置的路点位置作为初始化位置
            var pathPoint = new GameObject(GetPathPointNameByIndex(index)).transform;
            var isExistPathPoint = IsExistPathPointByIndex(index);
            var pathPointPosition = PathPointStartPos;
            if(isExistPathPoint && PathPointList[index] != null)
            {
                pathPointPosition = PathPointList[index].position;
            }
            pathPoint.position = pathPointPosition;
            pathPoint.gameObject.AddComponent<PathPointData>();
            pathPoint.SetParent(PathPointParentNode);
            return pathPoint;
        }

        /// <summary>
        /// 获取指定路点索引的节点名
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetPathPointNameByIndex(int index)
        {
            return $"PathPoint({index})";
        }

        /// <summary>
        /// 是否有路点
        /// </summary>
        /// <returns></returns>
        private bool HasPathPoint()
        {
            return PathPointList.Count > 0;
        }

        /// <summary>
        /// 指定索引是否存在路点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool IsExistPathPointByIndex(int index)
        {
            return index >= 0 && index < PathPointList.Count;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 场景Gizmos绘制
        /// </summary>
        private void OnDrawGizmos()
        {
            DrawPathPointSpheres();
            DrawPathPointIcons();
            DrawPathPointLabels();
        }

        /// <summary>
        /// 绘制路点球体
        /// </summary>
        private void DrawPathPointSpheres()
        {
            var preGimozColor = Gizmos.color;
            Gizmos.color = PathPointSphereColor;
            for(int i = PathPointList.Count - 1; i >= 0; i--)
            {
                var pathPoint = PathPointList[i];
                if(pathPoint != null)
                {
                    Gizmos.DrawWireSphere(pathPoint.position, Size)
                }
            }
            Gizmos.color = preGimozColor;
        }

        /// <summary>
        /// 绘制路点图标
        /// </summary>
        private void DrawPathPointIcons()
        {

        }

        /// <summary>
        /// 绘制路点标签
        /// </summary>
        private void DrawPathPointLabels()
        {

        }
#endif
    }
}