-- Script para agregar columnas de aprobadores a la tabla rendiciones
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

-- Agregar índices para mejorar el rendimiento
ALTER TABLE rendiciones 
ADD INDEX IF NOT EXISTS idx_aprobador_1_id (aprobador_1_id),
ADD INDEX IF NOT EXISTS idx_aprobador_2_id (aprobador_2_id),
ADD INDEX IF NOT EXISTS idx_estado (estado),
ADD INDEX IF NOT EXISTS idx_fecha_creacion (fecha_creacion);

-- Agregar restricciones de clave foránea si no existen
-- Nota: Estas restricciones asumen que los usuarios aprobadores existen en la tabla usuarios

-- Restricción para aprobador_1_id
ALTER TABLE rendiciones 
ADD CONSTRAINT IF NOT EXISTS fk_rendiciones_aprobador_1 
FOREIGN KEY (aprobador_1_id) REFERENCES usuarios(id) ON DELETE SET NULL;

-- Restricción para aprobador_2_id
ALTER TABLE rendiciones 
ADD CONSTRAINT IF NOT EXISTS fk_rendiciones_aprobador_2 
FOREIGN KEY (aprobador_2_id) REFERENCES usuarios(id) ON DELETE SET NULL;

-- Habilitar modo seguro nuevamente
SET SQL_SAFE_UPDATES = 1;

-- Verificar que las columnas se agregaron correctamente
DESCRIBE rendiciones; 