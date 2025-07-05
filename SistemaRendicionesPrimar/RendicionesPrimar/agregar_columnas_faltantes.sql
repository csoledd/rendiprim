-- Script para agregar columnas faltantes
USE rendiciones_primar;

-- Agregar columna apellidos a la tabla aprobadores si no existe
ALTER TABLE aprobadores 
ADD COLUMN apellidos VARCHAR(100) NULL AFTER nombre;

-- Agregar columnas nombre y apellidos a la tabla rendiciones si no existen
ALTER TABLE rendiciones 
ADD COLUMN nombre VARCHAR(100) NULL AFTER usuario_id,
ADD COLUMN apellidos VARCHAR(100) NULL AFTER nombre;

-- Actualizar datos existentes en aprobadores
-- Si hay datos en NombreCompleto, dividirlos en nombre y apellidos
UPDATE aprobadores 
SET apellidos = '' 
WHERE apellidos IS NULL;

-- Actualizar datos existentes en rendiciones
-- Obtener nombre y apellidos del usuario asociado
UPDATE rendiciones r
JOIN aprobadores a ON r.usuario_id = a.id
SET r.nombre = a.nombre,
    r.apellidos = a.apellidos
WHERE r.nombre IS NULL OR r.apellidos IS NULL;

-- Verificar que las columnas se agregaron correctamente
DESCRIBE aprobadores;
DESCRIBE rendiciones; 