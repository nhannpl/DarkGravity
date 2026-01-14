export interface Story {
  id: string; // Guid as string
  title: string;
  author: string;
  bodyText: string;
  url: string;
  aiAnalysis: string;
  scaryScore: number;
  fetchedAt: string;
  upvotes: number;
}
