using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IParticle
{
    Vector3 direction { get; set; }
    float speed { get; set; }
    
    float lifeTime { get; set; }
    float lifeSpan { get; set; }
}
