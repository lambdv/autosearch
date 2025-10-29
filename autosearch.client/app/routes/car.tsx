import type { Route } from "./+types/home";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "New React Router App" },
    { name: "description", content: "Welcome to React Router!" },
  ];
}

export default function Car({params}: {params: {id: string}}) {
  let id: string = params.id;
  return <p>
    {id}
  </p>;
}
