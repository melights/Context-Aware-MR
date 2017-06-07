/* *****************************************************************************
 * 
 *								EDUCATION RESEARCH GROUP
 *							MORGRIDGE INSTITUTE FOR RESEARCH
 * 			
 * 				
 * Copyright (c) 2012 EDUCATION RESEARCH, MORGRIDGE INSTITUTE FOR RESEARCH
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated  * documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 *  
 * 
 ******************************************************************************/
using UnityEngine;
using System.Collections;

[System.Serializable]
public class Triangle  : UnityEngine.Object
{
	public Vector3 pt0;
	public Vector3 pt1;
	public Vector3 pt2;
	
	public Vector2 uv_pt0;
	public Vector2 uv_pt1;
	public Vector2 uv_pt2;
	public Transform trans;
	
	public Triangle (Vector3 pt0, Vector3 pt1, Vector3 pt2, Vector2 uv_pt0, Vector2 uv_pt1, Vector2 uv_pt2, Transform trans)
	{
		this.pt0 = pt0;
		this.pt1 = pt1;
		this.pt2 = pt2;
		this.uv_pt0 = uv_pt0;
		this.uv_pt1 = uv_pt1;
		this.uv_pt2 = uv_pt2;
		this.trans = trans;
		UpdateVerts();
	}
	
	public void UpdateVerts(){
		pt0 = trans.TransformPoint(pt0);
		pt1 = trans.TransformPoint(pt1);
		pt2 = trans.TransformPoint(pt2);
	}
	

	
	

}
