using System.Linq;
using UnityEngine;

public class Example : MonoBehaviour {
    public string h =  "hola"; 
    void Start() {
        Debug.Log("jiji");
        Print(h);
    }

    public string  Print(string h ) {
        if (Contar(h) == 0) {
            Debug.Log(h + "1");
            return h;
        }
        Debug.Log(MostrarUltimoChar(h) + Print(EliminarUltimoChar(h)));
        return h;


    }

    int Contar(string h) {
       return h.Length;
    }

    public string MostrarUltimoChar(string h) {
        if (!string.IsNullOrEmpty(h)) {
            return h[h.Length - 1].ToString();
        }
        return "";
    }

    public string EliminarUltimoChar(string h) {
        if (!string.IsNullOrEmpty(h)) {
            return h.Substring(0, h.Length - 1);
        }
        return "";
    }

}
