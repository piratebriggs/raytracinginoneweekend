using System;
using System.Numerics;

namespace SimpleScene
{
	public struct SSSphere : IEquatable<SSSphere>
	{
		public Vector3 center;
		public float radius;

		public SSSphere (Vector3 _center, float _radius)
		{
			center = _center;
			radius = _radius;
		}

		public bool Equals(SSSphere other)
		{
			return this.center == other.center
				&& this.radius == other.radius;
		}

		public bool IntersectsSphere(SSSphere other)
		{
			float addedR = this.radius + other.radius;
			float addedRSq = addedR * addedR;
			float distSq = (other.center - this.center).LengthSquared();
			return addedRSq >= distSq;
		}

		public bool IntersectsRay (ref SSRay worldSpaceRay, out float distanceAlongRay)
		{
			float distanceToSphereOrigin = DistanceToLine(
				worldSpaceRay, this.center, out distanceAlongRay);
			return distanceToSphereOrigin <= this.radius;
		}

        /// <summary>
        /// Distance from a ray to a point at the closest spot. The ray is assumed to be infinite length.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float DistanceToLine(SSRay ray, Vector3 point, out float distanceAlongRay)
        {

            // http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line

            Vector3 a = ray.pos;
            Vector3 n = ray.dir;
            Vector3 p = point;

            var t = Vector3.Dot((a - p), n);
            distanceAlongRay = -t;
            return ((a - p) - t * n).Length();
        }

        public bool IntersectsAABB(SSAABB aabb)
		{
			return aabb.IntersectsSphere (this);
		}

		public SSAABB ToAABB()
		{
			Vector3 rvec = new Vector3 (radius);
			return new SSAABB (center - rvec, center + rvec);
		}
	}
}

