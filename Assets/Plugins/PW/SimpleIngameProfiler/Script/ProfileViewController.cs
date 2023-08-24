using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Playwith.TA
{
    public class ProfileViewController : MonoBehaviour
    {
        private int _enableIndex = 0;
        [SerializeField] private List<GameObject> profilePanelList = new List<GameObject>();
        private ProfileManager _profileManager;
        
        public RectTransform targetUI;  // 이동할 UI 요소

        [SerializeField] private Vector2[] targetPos;
        private int _targetPosIndex = 0;

        private void Start()
        {
            InitProfile();
        }

        private void InitProfile()
        {
            _profileManager = GetComponent<ProfileManager>();
            _enableIndex = (int)_profileManager.profilerState;
            AllPanelOff();
            SetActivePanel(_enableIndex);
        }
        
        public void OnChangeNext()
        {
            AllPanelOff();
            _enableIndex++;

            if (_enableIndex >= profilePanelList.Count)
            {
                _enableIndex = 0;
            }
            SetActivePanel(_enableIndex);
        }
        public void OnChangePrevious()
        {
            AllPanelOff();
            _enableIndex--;
            
            if (_enableIndex <= -1)
            {
                _enableIndex = profilePanelList.Count - 1;
            }
            SetActivePanel(_enableIndex);
        }
        
        private void AllPanelOff()
        {
            foreach (var panel in profilePanelList)
            {
                panel.SetActive(false);
            }
        }

        private void SetActivePanel(int number)
        {
            profilePanelList[number].SetActive(true);
            _profileManager.SetActiveProfile((ProfilerState)number);
        }

        public void OnMoveButtonClick()
        {
            targetUI.anchoredPosition = targetPos[_targetPosIndex];
            _targetPosIndex++;
            if (_targetPosIndex >= targetPos.Length)
            {
                _targetPosIndex = 0;
            }
        }
    }
}

