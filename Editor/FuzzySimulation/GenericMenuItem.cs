using UnityEditor;
using UnityEngine;

public class GenericMenuItem {
    
    private GUIContent content;
    private GenericMenu.MenuFunction func;

    public GenericMenuItem(GUIContent content, GenericMenu.MenuFunction func) {
        this.content = content;
        this.func = func;
    }
    
    public GUIContent GetContent() {
        return this.content;
    }

    public GenericMenu.MenuFunction GetFunc() {
        return this.func;
    }

}