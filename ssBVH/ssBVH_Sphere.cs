using System;
using System.Collections.Generic;
using System.Numerics;
using raytracinginoneweekend;


namespace SimpleScene.Util.ssBVH
{
	public class SphereBVHNodeAdaptor : SSBVHNodeAdaptor<Sphere>
	{
        protected ssBVH<Sphere> _bvh;
		protected Dictionary <Sphere, ssBVHNode<Sphere>> _sphereToLeafMap 
			= new Dictionary <Sphere, ssBVHNode<Sphere>>();
        public ssBVH<Sphere> BVH { get { return _bvh; } }

		public void setBVH(ssBVH<Sphere> bvh)
		{
			_bvh = bvh;
		}

		public Vector3 objectpos(Sphere sphere)
		{
			return sphere.Center;
		}

		public float radius(Sphere sphere)
		{
			return sphere.Radius;
		}

		public void checkMap(Sphere sphere) 
		{
			if (!_sphereToLeafMap.ContainsKey (sphere)) {
				throw new Exception("missing map for a shuffled child");
			}
		}

		public void unmapObject(Sphere sphere) 
		{
			_sphereToLeafMap.Remove(sphere);
		}

		public void mapObjectToBVHLeaf(Sphere sphere, ssBVHNode<Sphere> leaf) 
		{  
			_sphereToLeafMap[sphere] = leaf;
		}

		public ssBVHNode<Sphere> getLeaf(Sphere sphere)
		{
			return _sphereToLeafMap [sphere];
		}
	}

	public class SphereBVH : ssBVH<Sphere>
	{
		public SphereBVH(int maxSpheresPerLeaf=1)
			: base(new SphereBVHNodeAdaptor(),
				   new List<Sphere>(),
				   maxSpheresPerLeaf)
		{
		}
	}
}

