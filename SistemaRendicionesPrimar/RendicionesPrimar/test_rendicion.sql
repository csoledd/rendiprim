-- Script de prueba para verificar la estructura de la tabla rendiciones
-- Ejecutar este script en MySQL para diagnosticar problemas

USE sistema_rendiciones;

-- Verificar si la tabla rendiciones existe
SHOW TABLES LIKE 'rendiciones';

-- Verificar la estructura de la tabla rendiciones
DESCRIBE rendiciones;

-- Verificar si hay datos en la tabla
SELECT COUNT(*) as total_rendiciones FROM rendiciones;

-- Verificar si hay usuarios con rol empleado
SELECT id, nombre, apellidos, rol FROM usuarios WHERE rol = 'empleado' LIMIT 5;

-- Verificar si hay aprobadores
SELECT id, nombre, apellidos, rol FROM usuarios WHERE rol IN ('aprobador1', 'aprobador2') LIMIT 5;

-- Verificar la estructura de la tabla archivos_adjuntos
DESCRIBE archivos_adjuntos;

-- Verificar la estructura de la tabla notificaciones
DESCRIBE notificaciones;

-- Verificar permisos de la base de datos
SHOW GRANTS FOR CURRENT_USER(); 