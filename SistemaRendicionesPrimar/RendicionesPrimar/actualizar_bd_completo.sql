-- Script completo para actualizar la base de datos del Sistema de Rendiciones
-- Ejecutar este script en la base de datos MySQL

-- Deshabilitar modo seguro temporalmente para permitir actualizaciones
SET SQL_SAFE_UPDATES = 0;

-- ===== ACTUALIZAR TABLA usuarios =====
-- Agregar columnas faltantes a la tabla usuarios si no existen
ALTER TABLE usuarios 
ADD COLUMN IF NOT EXISTS nombre_completo VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS telefono VARCHAR(20) NULL,
ADD COLUMN IF NOT EXISTS cargo VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS departamento VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS activo BOOLEAN DEFAULT TRUE,
ADD COLUMN IF NOT EXISTS fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP;

-- ===== ACTUALIZAR TABLA rendiciones =====
-- Agregar columnas de aprobadores si no existen
ALTER TABLE rendiciones 
ADD COLUMN IF NOT EXISTS aprobador_1_id INT NULL,
ADD COLUMN IF NOT EXISTS aprobador_2_id INT NULL,
ADD COLUMN IF NOT EXISTS fecha_aprobacion_1 DATETIME NULL,
ADD COLUMN IF NOT EXISTS fecha_aprobacion_2 DATETIME NULL,
ADD COLUMN IF NOT EXISTS fecha_pago DATETIME NULL,
ADD COLUMN IF NOT EXISTS comentarios_aprobador TEXT NULL,
ADD COLUMN IF NOT EXISTS aprobado_por_1 INT NULL,
ADD COLUMN IF NOT EXISTS aprobado_por_2 INT NULL,
ADD COLUMN IF NOT EXISTS nombre_completo VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS rut VARCHAR(20) NULL,
ADD COLUMN IF NOT EXISTS telefono VARCHAR(20) NULL,
ADD COLUMN IF NOT EXISTS cargo VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS departamento VARCHAR(100) NULL;

-- ===== VERIFICAR TABLA archivos_adjuntos =====
-- Crear tabla si no existe
CREATE TABLE IF NOT EXISTS archivos_adjuntos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    rendicion_id INT NOT NULL,
    nombre_archivo VARCHAR(255) NOT NULL,
    ruta_archivo VARCHAR(500) NOT NULL,
    tipo_archivo VARCHAR(50) NULL,
    tamano_archivo BIGINT NULL,
    fecha_subida DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ===== VERIFICAR TABLA notificaciones =====
-- Crear tabla si no existe
CREATE TABLE IF NOT EXISTS notificaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    rendicion_id INT NULL,
    mensaje TEXT NOT NULL,
    leido BOOLEAN DEFAULT FALSE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Habilitar modo seguro nuevamente
SET SQL_SAFE_UPDATES = 1;

-- Verificar la estructura de las tablas
SELECT 'usuarios' as tabla, COUNT(*) as columnas FROM information_schema.columns WHERE table_name = 'usuarios' AND table_schema = DATABASE();
SELECT 'rendiciones' as tabla, COUNT(*) as columnas FROM information_schema.columns WHERE table_name = 'rendiciones' AND table_schema = DATABASE();
SELECT 'archivos_adjuntos' as tabla, COUNT(*) as columnas FROM information_schema.columns WHERE table_name = 'archivos_adjuntos' AND table_schema = DATABASE();
SELECT 'notificaciones' as tabla, COUNT(*) as columnas FROM information_schema.columns WHERE table_name = 'notificaciones' AND table_schema = DATABASE();

-- Mostrar estructura de la tabla rendiciones
DESCRIBE rendiciones; 