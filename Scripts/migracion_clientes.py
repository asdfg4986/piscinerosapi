import pandas as pd
import requests
import urllib3

# Desactivar advertencias SSL en entorno de desarrollo local
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

URL_API = "https://localhost:7168/api/clientes" # Verifica que el puerto sea el correcto
ARCHIVO_EXCEL = "../Datos/clientes_prueba.xlsx" # El nombre exacto de tu archivo

def procesar_clientes():
    print(f"Cargando el archivo {ARCHIVO_EXCEL}...")
    
    try:
        # Leemos el Excel
        df = pd.read_excel(ARCHIVO_EXCEL)

        # Elimina las filas donde TODOS los valores sean nulos (NaN)
        df = df.dropna(how='all')
        
        # Llenamos los valores nulos (NaN) con strings vacíos para campos de texto
        df = df.fillna("")
        
        contador_exitos = 0
        contador_errores = 0

        for index, row in df.iterrows():
            # Limpieza específica para campos numéricos que podrían venir vacíos
            # Si 'Visitas' está vacío, asumimos 0. Si viene como texto, lo forzamos a entero.
            visitas = row["VISITAS"] if row["VISITAS"] != "" else 0
            visitas = int(visitas)

            # Construimos el diccionario mapeando las columnas del Excel a las propiedades de C#
            # IMPORTANTE: Los nombres entre corchetes row["..."] deben coincidir EXACTAMENTE con tu Excel
            payload = {
                "nombre": str(row["CLIENTE"]).strip(),
                "direccion": str(row["DIRECCIÓN"]).strip(),
                "comuna": str(row["COMUNA"]).strip(),
                "telefono": str(row["CELULAR"]).strip(),
                "visitasPorMes": visitas,
                "diaPreferido": str(row["Día"]).strip(),
                "observaciones": str(row["Observaciones"]).strip()
            }
            
            # Hacemos la petición POST a la API
            response = requests.post(URL_API, json=payload, verify=False)
            
            if response.status_code == 201:
                print(f"✅ Registrado: {payload['nombre']} ({payload['comuna']})")
                contador_exitos += 1
            else:
                print(f"❌ Error con {payload['nombre']}: {response.status_code} - {response.text}")
                contador_errores += 1
                
        print("\n--- RESUMEN DE MIGRACIÓN ---")
        print(f"Clientes guardados exitosamente: {contador_exitos}")
        print(f"Errores encontrados: {contador_errores}")

    except FileNotFoundError:
        print(f"❌ Error: No se encontró el archivo '{ARCHIVO_EXCEL}'. Asegúrate de que esté en la misma carpeta que este script.")
    except Exception as e:
        print(f"❌ Ocurrió un error general: {e}")

if __name__ == "__main__":
    procesar_clientes()