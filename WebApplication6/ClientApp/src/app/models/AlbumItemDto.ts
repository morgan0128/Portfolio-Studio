// import type {PhotoDisplayDto} from './PhotoDisplayDto';
import type {PhotoDto} from './PhotoDto';
// import type {PhotoDisplayCollectionDto} from './PhotoDisplayCollectionDto';

// export type AlbumItemDto =
//   | {
//       kind: 'photoDisplay';
//       id: number;
//       order: number;
//       content: PhotoDisplayDto;
//     }
//   | {
//       kind: 'photoDisplayCollection';
//       id: number;
//       order: number;
//       content: PhotoDisplayCollectionDto;
//     };

export type ItemBase = { id: number; order: number };

export type PhotoDisplayDto = {
  photo: PhotoDto;
  photoDisplayCollectionId: number | null;
  displaysName: boolean;
  displaysDescription: boolean;
  displaysYearContentCreated: boolean;
};

export type PhotoDisplayItem = ItemBase & { content: PhotoDisplayDto };

export type PhotoDisplayCollectionDto = {
  displayMode: number;
  photoDisplays: PhotoDisplayItem[];
};

export type PhotoDisplayCollectionItem = ItemBase & { content: PhotoDisplayCollectionDto };

export type AlbumItemDto =
  | (PhotoDisplayItem & { kind: 'photoDisplay' })
  | (PhotoDisplayCollectionItem & { kind: 'photoDisplayCollection'}
  );



// export type ItemBase = { id: number; order: number };
//
// // export type PhotoDisplay = {
// //   photo: PhotoDto;
// //   photoDisplayCollectionId: number | null;
// //   displaysName: boolean;
// //   displaysDescription: boolean;
// //   displaysYearContentCreated: boolean;
// // };
//
// export type PhotoDisplay = ItemBase & {
//   photo: PhotoDto;
//   photoDisplayCollectionId: number | null;
//   displaysName: boolean;
//   displaysDescription: boolean;
//   displaysYearContentCreated: boolean;
// };
//
// export type PhotoDisplayCollection = {
//   displayMode: number;
//   photoDisplays: PhotoDisplayItem[];
// };
//
// export type AlbumItemDto =
//   | (PhotoDisplayItem & { kind: 'photoDisplay' })
//   | (ItemBase & {
//   kind: 'photoDisplayCollection';
//   content: PhotoDisplayCollection;
// });
