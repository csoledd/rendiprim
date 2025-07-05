-- Script final para actualizar la base de datos del Sistema de Rendiciones
-- Ejecutar este script en MySQL para corregir todos los problemas

USE sistema_rendiciones;

-- Deshabilitar modo seguro temporalmente
SET SQL_SAFE_UPDATES = 0;

-- ===== ACTUALIZAR TABLA usuarios =====
-- Agregar columnas faltantes a la tabla usuarios
ALTER TABLE usuarios 
ADD COLUMN IF NOT EXISTS nombre_completo VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS telefono VARCHAR(20) NULL,
ADD COLUMN IF NOT EXISTS cargo VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS departamento VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS activo BOOLEAN DEFAULT TRUE,
ADD COLUMN IF NOT EXISTS fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP;

-- ===== ACTUALIZAR TABLA rendiciones =====
-- Agregar todas las columnas necesarias para rendiciones
ALTER TABLE rendiciones 
ADD COLUMN IF NOT EXISTS aprobador_1_id INT NULL,
ADD COLUMN IF NOT EXISTS aprobador_2_id INT NULL,
ADD COLUMN IF NOT EXISTS fecha_aprobacion_1 DATETIME NULL,
ADD COLUMN IF NOT EXISTS fecha_aprobacion_2 DATETIME NULL,
ADD COLUMN IF NOT EXISTS fecha_pago DATETIME NULL,
ADD COLUMN IF NOT EXISTS comentarios_aprobador TEXT NULL,
ADD COLUMN IF NOT EXISTS aprobado_por_1 INT NULL,
ADD COLUMN IF NOT EXISTS aprobado_por_2 INT NULL,
ADD COLUMN IF NOT EXISTS nombre VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS apellidos VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS rut VARCHAR(20) NULL,
ADD COLUMN IF NOT EXISTS telefono VARCHAR(20) NULL,
ADD COLUMN IF NOT EXISTS cargo VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS departamento VARCHAR(100) NULL;

-- ===== CREAR TABLA archivos_adjuntos SI NO EXISTE =====
CREATE TABLE IF NOT EXISTS archivos_adjuntos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    rendicion_id INT NOT NULL,
    nombre_archivo VARCHAR(255) NOT NULL,
    ruta_archivo VARCHAR(500) NOT NULL,
    tipo_archivo VARCHAR(50) NULL,
    tamano_archivo BIGINT NULL,
    fecha_subida DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_rendicion_id (rendicion_id)
);

-- ===== CREAR TABLA notificaciones SI NO EXISTE =====
CREATE TABLE IF NOT EXISTS notificaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    rendicion_id INT NULL,
    mensaje TEXT NOT NULL,
    leido BOOLEAN DEFAULT FALSE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    tipo_rol VARCHAR(20) NULL,
    INDEX idx_usuario_id (usuario_id),
    INDEX idx_rendicion_id (rendicion_id),
    INDEX idx_leido (leido)
);

-- ===== CREAR TABLA aprobadores SI NO EXISTE =====
CREATE TABLE IF NOT EXISTS aprobadores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NULL,
    email VARCHAR(100) NOT NULL,
    rol VARCHAR(20) NOT NULL,
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Habilitar modo seguro nuevamente
SET SQL_SAFE_UPDATES = 1;

-- Verificar la estructura final
SELECT '=== ESTRUCTURA DE TABLAS ===' as info;
DESCRIBE usuarios;
DESCRIBE rendiciones;
DESCRIBE archivos_adjuntos;
DESCRIBE notificaciones;
DESCRIBE aprobadores;

-- Verificar datos existentes
SELECT '=== DATOS EXISTENTES ===' as info;
SELECT COUNT(*) as total_usuarios FROM usuarios;
SELECT COUNT(*) as total_rendiciones FROM rendiciones;
SELECT COUNT(*) as total_archivos FROM archivos_adjuntos;
SELECT COUNT(*) as total_notificaciones FROM notificaciones;

-- Verificar usuarios por rol
SELECT '=== USUARIOS POR ROL ===' as info;
SELECT rol, COUNT(*) as cantidad FROM usuarios GROUP BY rol;

SELECT '=== ACTUALIZACIÓN COMPLETADA ===' as mensaje; 