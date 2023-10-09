using UnityEngine;
using UnityEngine.EventSystems;

namespace RuntimeInspectorNamespace
{
	public class PointerEventListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
	{
		public delegate void PointerEvent( PointerEventData eventData );

		public event PointerEvent PointerDown, PointerUp, PointerClick,PointerEnter,PointerExit;

		void IPointerDownHandler.OnPointerDown( PointerEventData eventData )
		{
			if( PointerDown != null )
				PointerDown( eventData );
		}

		void IPointerUpHandler.OnPointerUp( PointerEventData eventData )
		{
			if( PointerUp != null )
				PointerUp( eventData );
		}

		void IPointerClickHandler.OnPointerClick( PointerEventData eventData )
		{
			if( PointerClick != null )
				PointerClick( eventData );
		}

        public void OnPointerEnter(PointerEventData eventData)
        {
			if (PointerEnter != null)
				PointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (PointerExit != null)
                PointerExit(eventData);
        }
    }
}