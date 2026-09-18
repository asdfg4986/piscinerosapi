import pandas as pd
import requests
import urllib3
import os
import glob

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

URL_API_CLIENTES = "https://localhost:7168/api/clientes"
URL_API_LOGIN = "https://localhost:7168/api/auth/login"
CARPETA_DATOS = "../Datos/"

ADMIN_EMAIL = "ejemplo@piscineros.cl"
ADMIN_PASSWORD = "contraseñasecreta123" 

def obtener_token():
    print("Iniciando sesión como Administrador...")
    payload = {"email": ADMIN_EMAIL, "password": ADMIN_PASSWORD}
    response = requests.post(URL_API_LOGIN, json=payload, verify=False)
    
    if response.status_code == 200:
        return response.json().get("token")
    else:
        print("Error al iniciar sesión. Revisa las credenciales.")
        return None

def limpiar_y_extraer_datos(df):
    # Estandarizamos los nombres de las columnas: en mayúsculas y sin espacios a los lados
    df.columns = df.columns.str.strip().str.upper()
    df = df.dropna(how='all').fillna("")
    
    clientes_list = []
    
    # Identificar la columna de observaciones dinámicamente
    columna_obs = "OBSERVACIONES"
    if "UNNAMED: 12" in df.columns and columna_obs not in df.columns:
        columna_obs = "UNNAMED: 12" # Caso del Excel de Cristobal

    for index, row in df.iterrows():
        # Saltamos filas donde no hay nombre de cliente
        if str(row.get("CLIENTE", "")).strip() == "":
            continue

        visitas = row.get("VISITAS", 0)
        visitas = int(visitas) if str(visitas).isdigit() else 0

        payload = {
            "nombre": str(row.get("CLIENTE", "")).strip(),
            "direccion": str(row.get("DIRECCIÓN", "")).strip(),
            "comuna": str(row.get("COMUNA", "")).strip(),
            "telefono": str(row.get("CELULAR", "")).strip(),
            "visitasPorMes": visitas,
            "diaPreferido": str(row.get("DÍA", "")).strip(),
            "observaciones": str(row.get(columna_obs, "")).strip(),
            "activo": True
        }
        clientes_list.append(payload)
        
    return clientes_list

def procesar_clientes():
    token = obtener_token()
    if not token:
        return

    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    archivos_excel = glob.glob(os.path.join(CARPETA_DATOS, "*.xlsx"))
    
    if not archivos_excel:
        print(f"No se encontraron archivos Excel en {CARPETA_DATOS}")
        return

    total_exitos = 0
    total_errores = 0

    for archivo in archivos_excel:
        print(f"\nProcesando archivo: {os.path.basename(archivo)}...")
        try:
            df = pd.read_excel(archivo)
            lista_clientes = limpiar_y_extraer_datos(df)

            for payload in lista_clientes:
                response = requests.post(URL_API_CLIENTES, json=payload, headers=headers, verify=False)
                
                if response.status_code == 201:
                    print(f"  [OK] Registrado: {payload['nombre']}")
                    total_exitos += 1
                else:
                    print(f"  [ERROR] {payload['nombre']}: {response.text}")
                    total_errores += 1
        except Exception as e:
            print(f"Error procesando el archivo {os.path.basename(archivo)}: {e}")

    print("\n==============================")
    print("--- RESUMEN DE MIGRACIÓN ---")
    print(f"Total clientes guardados: {total_exitos}")
    print(f"Total errores encontrados: {total_errores}")
    print("==============================")

if __name__ == "__main__":
    procesar_clientes()