-- Script de diagnóstico específico para la estructura real
-- Basado en la estructura actual: usuarios tiene 'apellido' y 'tipo_usuario'

USE sistema_rendiciones;

-- ===== PASO 1: Verificar estructura actual =====
SELECT '=== ESTRUCTURA ACTUAL ===' as info;
SHOW TABLES;

-- Verificar estructura de usuarios
SELECT '=== ESTRUCTURA USUARIOS ===' as info;
DESCRIBE usuarios;

-- Verificar estructura de rendiciones
SELECT '=== ESTRUCTURA RENDICIONES ===' as info;
DESCRIBE rendiciones;

-- ===== PASO 2: Verificar datos existentes =====
SELECT '=== DATOS EXISTENTES ===' as info;

-- Verificar usuarios
SELECT COUNT(*) as total_usuarios FROM usuarios;

-- Verificar usuarios por tipo_usuario
SELECT '=== USUARIOS POR TIPO ===' as info;
SELECT tipo_usuario, COUNT(*) as cantidad FROM usuarios GROUP BY tipo_usuario;

-- Verificar usuarios con tipo_usuario = 1 (empleados)
SELECT '=== EMPLEADOS ===' as info;
SELECT id, nombre, apellido, email, tipo_usuario FROM usuarios WHERE tipo_usuario = 1 LIMIT 5;

-- Verificar usuarios con tipo_usuario = 2 (aprobador1)
SELECT '=== APROBADORES 1 ===' as info;
SELECT id, nombre, apellido, email, tipo_usuario FROM usuarios WHERE tipo_usuario = 2 LIMIT 5;

-- Verificar usuarios con tipo_usuario = 3 (aprobador2)
SELECT '=== APROBADORES 2 ===' as info;
SELECT id, nombre, apellido, email, tipo_usuario FROM usuarios WHERE tipo_usuario = 3 LIMIT 5;

-- Verificar rendiciones
SELECT COUNT(*) as total_rendiciones FROM rendiciones;

-- Verificar archivos_adjuntos
SHOW TABLES LIKE 'archivos_adjuntos';
SELECT COUNT(*) as total_archivos FROM archivos_adjuntos;

-- Verificar notificaciones
SHOW TABLES LIKE 'notificaciones';
SELECT COUNT(*) as total_notificaciones FROM notificaciones;

-- ===== PASO 3: Verificar columnas específicas en rendiciones =====
SELECT '=== COLUMNAS CRÍTICAS EN RENDICIONES ===' as info;

-- Verificar si existen las columnas necesarias para el funcionamiento
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = DATABASE() 
AND TABLE_NAME = 'rendiciones' 
AND COLUMN_NAME IN (
    'aprobador_1_id', 
    'aprobador_2_id', 
    'fecha_aprobacion_1', 
    'fecha_aprobacion_2', 
    'fecha_pago', 
    'comentarios_aprobador',
    'aprobado_por_1',
    'aprobado_por_2',
    'nombre',
    'apellidos',
    'rut',
    'telefono',
    'cargo',
    'departamento'
);

-- ===== PASO 4: Verificar versión de MySQL =====
SELECT '=== INFORMACIÓN DEL SISTEMA ===' as info;
SELECT VERSION() as mysql_version;

-- ===== PASO 5: Verificar permisos =====
SHOW GRANTS FOR CURRENT_USER();

SELECT '=== DIAGNÓSTICO COMPLETADO ===' as mensaje; 