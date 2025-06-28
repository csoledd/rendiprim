-- Script simple para agregar columnas de aprobadores a la tabla rendiciones
-- Ejecutar este script en la base de datos MySQL

-- Deshabilitar modo seguro temporalmente para permitir actualizaciones
SET SQL_SAFE_UPDATES = 0;

-- Agregar columnas de aprobadores si no existen
ALTER TABLE rendiciones 
ADD COLUMN IF NOT EXISTS aprobador_1_id INT NULL,
ADD COLUMN IF NOT EXISTS aprobador_2_id INT NULL,
ADD COLUMN IF NOT EXISTS fecha_aprobacion_1 DATETIME NULL,
ADD COLUMN IF NOT EXISTS fecha_aprobacion_2 DATETIME NULL,
ADD COLUMN IF NOT EXISTS fecha_pago DATETIME NULL,
ADD COLUMN IF NOT EXISTS comentarios_aprobador TEXT NULL,
ADD COLUMN IF NOT EXISTS aprobado_por_1 INT NULL,
ADD COLUMN IF NOT EXISTS aprobado_por_2 INT NULL;

-- Habilitar modo seguro nuevamente
SET SQL_SAFE_UPDATES = 1;

-- Verificar que las columnas se agregaron correctamente
DESCRIBE rendiciones; 