-- Script simple para actualizar la base de datos
-- Ejecutar este script en MySQL

USE rendiciones_primar;

-- Agregar las nuevas columnas
ALTER TABLE usuarios 
ADD COLUMN IF NOT EXISTS nombre_completo VARCHAR(100) NOT NULL DEFAULT '' AFTER nombre,
ADD COLUMN IF NOT EXISTS rut VARCHAR(20) NOT NULL DEFAULT '' AFTER nombre_completo,
ADD COLUMN IF NOT EXISTS telefono VARCHAR(20) NULL AFTER email,
ADD COLUMN IF NOT EXISTS cargo VARCHAR(100) NULL AFTER rol,
ADD COLUMN IF NOT EXISTS departamento VARCHAR(100) NULL AFTER cargo;

-- Actualizar registros existentes
UPDATE usuarios SET 
nombre_completo = nombre,
rut = '00000000-0'
WHERE nombre_completo = '' OR rut = '';

-- Verificar cambios
SELECT 'Base de datos actualizada exitosamente' AS Mensaje; 