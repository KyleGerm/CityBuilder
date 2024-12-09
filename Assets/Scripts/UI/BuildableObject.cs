using Game.Tools;
using Game.Interfaces;
using UnityEngine;
using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>
    /// Script which handles object instantiation, validation, and handing the object to the buildMenu
    /// </summary>
    public class BuildableObject : MonoBehaviour, IWouldLikeToKnowWhenYouHaveBeenDestroyed
    {
        private BuildMenu menu;
        private GameObject obj;

        private float buildPrice;
        private float buildMulti;
        private int objectsBuilt;
        private List<GameObject> objects = new();
        public int BuildCost => objectsBuilt > 0? buildPrice.ToInt(Round.Down) : 0;

        /// <summary>
        /// Sets the object handed to it as the one it will instance when asked.
        /// </summary>
        /// <param name="obj"></param>
        public void SetObj(GameObject obj)
        {
            if (this.obj != null) return;
            this.obj = obj;
            obj.GetComponent<IBuildable>().SyncValues(ref buildPrice, ref buildMulti);
        }

        /// <summary>
        /// Answers to this manager
        /// </summary>
        /// <param name="caller"></param>
        public void SetMenuAs(BuildMenu caller)
        {
            menu = caller;
        }

        /// <summary>
        /// Creates an instance of its object, and waits for it to be validated. 
        /// Destroys the instantiated object if it cannot be taken. 
        /// </summary>
        public void BuildNewObject()
        {
             CreateNewInstance(out GameObject buildableObj);
            objects.Add(buildableObj);
            if (!menu.BeginPlacementRoutine(buildableObj,this))
            {
                Destroy(buildableObj);
            }
        }

        public BuildableObject CreateNewInstance(out GameObject instance)
        {
            GameObject buildableObj = Instantiate(obj);
            if (buildableObj.transform.localScale != Vector3.one)
            {
                buildableObj.transform.localScale /= 10;
            }
            instance = buildableObj;
            instance.gameObject.GetComponent<IBuildable>().SetSpawner(this);
            return this;
        }
        /// <summary>
        /// Multiplies the build cost by the cost multiplier
        /// </summary>
        public void IncreaseBuildCost()
        {
            objectsBuilt++;
            if (objectsBuilt <= 1) return;
            buildPrice *= buildMulti;
        }

        public void ItemWasDestroyed(GameObject item)
        {
            if(objectsBuilt < 1 || !objects.Contains(item)) return;
            objectsBuilt--;
            buildPrice /= buildMulti;
        }
    }
}
