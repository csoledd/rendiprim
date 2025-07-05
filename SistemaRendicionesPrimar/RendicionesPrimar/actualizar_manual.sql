-- Script de actualización manual paso a paso
-- Ejecutar cada comando por separado para evitar errores

USE sistema_rendiciones;

-- ===== PASO 1: Verificar estructura actual =====
SHOW TABLES;
DESCRIBE usuarios;
DESCRIBE rendiciones;

-- ===== PASO 2: Agregar columnas a usuarios (ejecutar uno por uno) =====

-- Agregar nombre_completo si no existe
ALTER TABLE usuarios ADD COLUMN nombre_completo VARCHAR(100) NULL;

-- Agregar telefono si no existe
ALTER TABLE usuarios ADD COLUMN telefono VARCHAR(20) NULL;

-- Agregar cargo si no existe
ALTER TABLE usuarios ADD COLUMN cargo VARCHAR(100) NULL;

-- Agregar departamento si no existe
ALTER TABLE usuarios ADD COLUMN departamento VARCHAR(100) NULL;

-- Agregar activo si no existe
ALTER TABLE usuarios ADD COLUMN activo BOOLEAN DEFAULT TRUE;

-- Agregar fecha_creacion si no existe
ALTER TABLE usuarios ADD COLUMN fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP;

-- ===== PASO 3: Agregar columnas a rendiciones (ejecutar uno por uno) =====

-- Agregar aprobador_1_id si no existe
ALTER TABLE rendiciones ADD COLUMN aprobador_1_id INT NULL;

-- Agregar aprobador_2_id si no existe
ALTER TABLE rendiciones ADD COLUMN aprobador_2_id INT NULL;

-- Agregar fecha_aprobacion_1 si no existe
ALTER TABLE rendiciones ADD COLUMN fecha_aprobacion_1 DATETIME NULL;

-- Agregar fecha_aprobacion_2 si no existe
ALTER TABLE rendiciones ADD COLUMN fecha_aprobacion_2 DATETIME NULL;

-- Agregar fecha_pago si no existe
ALTER TABLE rendiciones ADD COLUMN fecha_pago DATETIME NULL;

-- Agregar comentarios_aprobador si no existe
ALTER TABLE rendiciones ADD COLUMN comentarios_aprobador TEXT NULL;

-- Agregar aprobado_por_1 si no existe
ALTER TABLE rendiciones ADD COLUMN aprobado_por_1 INT NULL;

-- Agregar aprobado_por_2 si no existe
ALTER TABLE rendiciones ADD COLUMN aprobado_por_2 INT NULL;

-- Agregar nombre si no existe
ALTER TABLE rendiciones ADD COLUMN nombre VARCHAR(100) NULL;

-- Agregar apellidos si no existe
ALTER TABLE rendiciones ADD COLUMN apellidos VARCHAR(100) NULL;

-- Agregar rut si no existe
ALTER TABLE rendiciones ADD COLUMN rut VARCHAR(20) NULL;

-- Agregar telefono si no existe
ALTER TABLE rendiciones ADD COLUMN telefono VARCHAR(20) NULL;

-- Agregar cargo si no existe
ALTER TABLE rendiciones ADD COLUMN cargo VARCHAR(100) NULL;

-- Agregar departamento si no existe
ALTER TABLE rendiciones ADD COLUMN departamento VARCHAR(100) NULL;

-- ===== PASO 4: Crear tablas si no existen =====

-- Crear tabla archivos_adjuntos
CREATE TABLE IF NOT EXISTS archivos_adjuntos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    rendicion_id INT NOT NULL,
    nombre_archivo VARCHAR(255) NOT NULL,
    ruta_archivo VARCHAR(500) NOT NULL,
    tipo_archivo VARCHAR(50) NULL,
    tamano_archivo BIGINT NULL,
    fecha_subida DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Crear tabla notificaciones
CREATE TABLE IF NOT EXISTS notificaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    rendicion_id INT NULL,
    mensaje TEXT NOT NULL,
    leido BOOLEAN DEFAULT FALSE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    tipo_rol VARCHAR(20) NULL
);

-- Crear tabla aprobadores
CREATE TABLE IF NOT EXISTS aprobadores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NULL,
    email VARCHAR(100) NOT NULL,
    rol VARCHAR(20) NOT NULL,
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ===== PASO 5: Verificar estructura final =====
SELECT '=== ESTRUCTURA FINAL ===' as info;
DESCRIBE usuarios;
DESCRIBE rendiciones;
DESCRIBE archivos_adjuntos;
DESCRIBE notificaciones;
DESCRIBE aprobadores;

-- ===== PASO 6: Verificar datos =====
SELECT '=== DATOS EXISTENTES ===' as info;
SELECT COUNT(*) as total_usuarios FROM usuarios;
SELECT COUNT(*) as total_rendiciones FROM rendiciones;
SELECT COUNT(*) as total_archivos FROM archivos_adjuntos;
SELECT COUNT(*) as total_notificaciones FROM notificaciones;

-- Verificar usuarios por rol
SELECT '=== USUARIOS POR ROL ===' as info;
SELECT rol, COUNT(*) as cantidad FROM usuarios GROUP BY rol;

SELECT '=== ACTUALIZACIÓN COMPLETADA ===' as mensaje; 