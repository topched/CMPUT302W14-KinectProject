<![CDATA[
    var selectedElement = 0;
    var currentX = 0;
    var currentY = 0;
    var currentMatrix = 0;

    function selectElement(evt) {
      selectedElement = evt.target;
      currentX = evt.clientX;
      currentY = evt.clientY;
      currentMatrix = selectedElement.getAttributeNS(null, "transform").slice(7,-1).split(' ');
    
      for(var i=0; i<currentMatrix.length; i++) {
        currentMatrix[i] = parseFloat(currentMatrix[i]);
      }
      
      selectedElement.setAttributeNS(null, "onmousemove", "moveElement(evt)");
      selectedElement.setAttributeNS(null, "onmouseout", "deselectElement(evt)");
      selectedElement.setAttributeNS(null, "onmouseup", "deselectElement(evt)");
    }
        
    function moveElement(evt) {
      var dx = evt.clientX - currentX;
      var dy = evt.clientY - currentY;
      currentMatrix[4] += dx;
      currentMatrix[5] += dy;
      
      selectedElement.setAttributeNS(null, "transform", "matrix(" + currentMatrix.join(' ') + ")");
      currentX = evt.clientX;
      currentY = evt.clientY;
    }
        
    function deselectElement(evt) {
      if(selectedElement != 0){
          selectedElement.removeAttributeNS(null, "onmousemove");
          selectedElement.removeAttributeNS(null, "onmouseout");
          selectedElement.removeAttributeNS(null, "onmouseup");
          selectedElement = 0;
          }
	}
]]> 