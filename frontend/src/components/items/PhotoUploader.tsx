import { useState, useRef, useCallback } from 'react';
import { HiPhotograph, HiX } from 'react-icons/hi';
import { useTranslation } from 'react-i18next';

interface PhotoUploaderProps {
  photos: File[];
  existingPhotos?: { id: string; url: string }[];
  onChange: (files: File[]) => void;
  onRemoveExisting?: (id: string) => void;
  maxPhotos?: number;
}

export default function PhotoUploader({
  photos,
  existingPhotos = [],
  onChange,
  onRemoveExisting,
  maxPhotos = 5,
}: PhotoUploaderProps) {
  const { t } = useTranslation();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [dragOver, setDragOver] = useState(false);
  const totalPhotos = existingPhotos.length + photos.length;

  const addFiles = useCallback((files: FileList | null) => {
    if (!files) return;
    const remaining = maxPhotos - totalPhotos;
    const newFiles = Array.from(files)
      .filter((f) => f.type.startsWith('image/'))
      .slice(0, remaining);
    if (newFiles.length > 0) {
      onChange([...photos, ...newFiles]);
    }
  }, [photos, onChange, maxPhotos, totalPhotos]);

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    addFiles(e.dataTransfer.files);
  };

  const removePhoto = (index: number) => {
    onChange(photos.filter((_, i) => i !== index));
  };

  return (
    <div>
      <div
        className={`border-2 border-dashed rounded-xl p-6 text-center cursor-pointer transition-colors ${
          dragOver ? 'border-primary bg-lavender/10' : 'border-lavender hover:border-primary/50'
        } ${totalPhotos >= maxPhotos ? 'opacity-50 pointer-events-none' : ''}`}
        onClick={() => fileInputRef.current?.click()}
        onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
        onDragLeave={() => setDragOver(false)}
        onDrop={handleDrop}
      >
        <HiPhotograph className="mx-auto h-10 w-10 text-lavender mb-2" />
        <p className="text-sm text-gray-500">{t('items.drag_drop')}</p>
        <p className="text-xs text-gray-400 mt-1">{t('items.max_photos')} ({totalPhotos}/{maxPhotos})</p>
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          multiple
          className="hidden"
          onChange={(e) => addFiles(e.target.files)}
        />
      </div>
      {(existingPhotos.length > 0 || photos.length > 0) && (
        <div className="flex gap-3 mt-4 flex-wrap">
          {existingPhotos.map((photo) => (
            <div key={photo.id} className="relative w-20 h-20 rounded-lg overflow-hidden">
              <img src={photo.url} alt="" className="w-full h-full object-cover" />
              {onRemoveExisting && (
                <button
                  onClick={() => onRemoveExisting(photo.id)}
                  className="absolute top-0.5 right-0.5 bg-red-500 text-white rounded-full p-0.5"
                >
                  <HiX className="h-3 w-3" />
                </button>
              )}
            </div>
          ))}
          {photos.map((file, index) => (
            <div key={index} className="relative w-20 h-20 rounded-lg overflow-hidden">
              <img src={URL.createObjectURL(file)} alt="" className="w-full h-full object-cover" />
              <button
                onClick={() => removePhoto(index)}
                className="absolute top-0.5 right-0.5 bg-red-500 text-white rounded-full p-0.5"
              >
                <HiX className="h-3 w-3" />
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
