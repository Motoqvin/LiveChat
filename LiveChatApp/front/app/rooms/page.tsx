"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";

const availableRooms = ["general", "sports", "music", "tech"];

export default function RoomsPage() {
  const router = useRouter();
  const [selectedRoom, setSelectedRoom] = useState<string | null>(null);

  const handleJoin = () => {
    if (selectedRoom) {
      router.push(`/?room=${selectedRoom}`);
    }
  };

  return (
    <div className="max-w-md mx-auto mt-20 p-4 border rounded shadow">
      <h1 className="text-2xl mb-4">Select a chat room</h1>
      <select
        value={selectedRoom ?? ""}
        onChange={(e) => setSelectedRoom(e.target.value)}
        className="border p-2 rounded w-full mb-4"
      >
        <option value="" disabled>
          Choose a room...
        </option>
        {availableRooms.map((room) => (
          <option key={room} value={room}>
            {room.charAt(0).toUpperCase() + room.slice(1)}
          </option>
        ))}
      </select>
      <button
        onClick={handleJoin}
        disabled={!selectedRoom}
        className={`w-full p-2 rounded text-white font-semibold ${
          selectedRoom ? "bg-blue-600 hover:bg-blue-700" : "bg-gray-400 cursor-not-allowed"
        }`}
      >
        Join Chat
      </button>
    </div>
  );
}
