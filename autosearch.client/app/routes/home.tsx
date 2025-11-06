import type { Route } from "./+types/home";
import { useState, useEffect } from "react";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "New React Router App" },
    { name: "description", content: "Welcome to React Router!" },
  ];
}

export default function Home() {
  const [cars, setCars] = useState<any[]>([]);

  useEffect(() => {
    async function fetchData() {
      let data = await getData();
      setCars(Array.isArray(data) ? data : []);
    }
    fetchData();
  }, []);

  return (
    <div>
      <nav>
        <p>autosearch</p>
      </nav>
      <div>
        <ul className="w-full bg-amber-700 mx-auto flex flex-col items-center">
          {cars && cars.length > 0 ? (
            cars.map((car, idx) => <CarListing carData={car} key={idx} />)
          ) : (
            <li>No cars found.</li>
          )}
        </ul>
      </div>
    </div>
  );
}

async function getData() {
  const response = await fetch("http://localhost:5030/cars/trademe");
  const data = await response.json();
  console.log(data)
  return data;
}

function CarListing({ carData }) {
  // defensively ensure images is always an array
  const images = Array.isArray(carData?.images) ? carData.images : [];
  // try both .name and .title for robustness; fall back to empty string
  const name = carData?.name || carData?.title || "";
  const price = carData?.price || "";

  return (
   <li className="w-full">
      <a href={carData.url} className="pd-2 bg-amber-900 relative flex">
        <img src={images[0] || ""} alt="" className="w-100px"/>
        <div className="w-1/2 bg-amber-600">
          <h1>{name}</h1>
          <p>Price: {price}</p>
        </div>
      </a>
   </li>
  );
}