// Copyright(C) David W. Jeske, 2013
// Released to the public domain.

using System;
using System.Collections.Generic;

using System.Drawing;

using System.Numerics;
using raytracinginoneweekend;

namespace SimpleScene.Util.ssBVH
{

    /// <summary>
    /// An adaptor for ssBVH to understand IHitable nodes.
    /// </summary>
    public class IHitableBVHNodeAdaptor : SSBVHNodeAdaptor<IHitable>
    {
        Dictionary<IHitable, ssBVHNode<IHitable>> ssToLeafMap = new Dictionary<IHitable, ssBVHNode<IHitable>>();

        public SSAABB boundingBox(IHitable obj)
        {
            return obj.BoundingBox;
        }

        public void checkMap(IHitable obj)
        {
            if (!ssToLeafMap.ContainsKey(obj))
            {
                throw new Exception("missing map for shuffled child");
            }
        }
        public void unmapObject(IHitable obj)
        {
            ssToLeafMap.Remove(obj);
        }
        public void mapObjectToBVHLeaf(IHitable obj, ssBVHNode<IHitable> leaf)
        {
            ssToLeafMap[obj] = leaf;
        }
        public ssBVHNode<IHitable> getLeaf(IHitable obj)
        {
            return ssToLeafMap[obj];
        }

        // the IHitable has changed, so notify the BVH leaf to refit for the object
        protected void obj_OnChanged(IHitable sender)
        {
            ssToLeafMap[sender].refit_ObjectChanged(this, sender);
        }

        ssBVH<IHitable> _BVH;
        public ssBVH<IHitable> BVH { get { return _BVH; } }

        public void setBVH(ssBVH<IHitable> BVH)
        {
            this._BVH = BVH;
        }

        public IHitableBVHNodeAdaptor() { }
    }
}
