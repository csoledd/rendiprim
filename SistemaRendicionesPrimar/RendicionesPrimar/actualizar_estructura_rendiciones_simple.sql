-- Script simple para actualizar la tabla rendiciones
USE sistema_rendiciones;

-- Agregar columna nombre si no existe
ALTER TABLE rendiciones ADD COLUMN IF NOT EXISTS nombre VARCHAR(100) NOT NULL DEFAULT '' AFTER descripcion;

