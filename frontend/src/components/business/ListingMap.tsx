import { useEffect } from "react";
import { MapContainer, TileLayer, Marker, Popup, useMap } from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

// Vite-friendly default-marker fix — Leaflet's CDN-relative icon paths break under bundlers.
// `?url` returns a hashed asset URL that survives prod builds.
import iconRetinaUrl from "leaflet/dist/images/marker-icon-2x.png?url";
import iconUrl from "leaflet/dist/images/marker-icon.png?url";
import shadowUrl from "leaflet/dist/images/marker-shadow.png?url";

L.Icon.Default.mergeOptions({
  iconRetinaUrl,
  iconUrl,
  shadowUrl,
});

interface ListingMapProps {
  latitude: number;
  longitude: number;
  label?: string;
  height?: number;
}

function Recenter({ lat, lng }: { lat: number; lng: number }) {
  const map = useMap();
  useEffect(() => {
    map.setView([lat, lng]);
  }, [map, lat, lng]);
  return null;
}

/**
 * Lightweight Leaflet map embedded in the listing detail page. Tiles come from
 * OpenStreetMap which requires the attribution string per their tile usage policy.
 */
export default function ListingMap({
  latitude,
  longitude,
  label,
  height = 280,
}: ListingMapProps) {
  return (
    <div
      style={{ height }}
      className="rounded-2xl overflow-hidden border border-lavender/30 dark:border-white/10"
    >
      <MapContainer
        center={[latitude, longitude]}
        zoom={14}
        scrollWheelZoom={false}
        style={{ height: "100%", width: "100%" }}
        attributionControl
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <Marker position={[latitude, longitude]}>
          {label && <Popup>{label}</Popup>}
        </Marker>
        <Recenter lat={latitude} lng={longitude} />
      </MapContainer>
    </div>
  );
}
