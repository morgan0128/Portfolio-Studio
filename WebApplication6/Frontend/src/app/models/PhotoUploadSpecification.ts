export class PhotoSpecification {
  name: string = '';
  description: string = '';
  yearContentCreated: number = 2003;
}

export class PhotoUploadSpecification {
  readonly #FileComponent: File;
  readonly #PhotoSpecComponent: PhotoSpecification;

  readonly #Form: FormData;

  constructor(file: File, photoSpecDTO: PhotoSpecification){
    this.#FileComponent = file;
    this.#PhotoSpecComponent = photoSpecDTO;

    const formData: FormData = new FormData();
    formData.append('file', this.#FileComponent, this.#FileComponent.name);
    formData.append('name', this.#PhotoSpecComponent.name);
    formData.append('description', this.#PhotoSpecComponent.description)
    formData.append('yearContentCreated', this.#PhotoSpecComponent.yearContentCreated.toString());

    this.#Form = formData;
  }

  asFormData(): FormData {
    const copy = new FormData();

    this.#Form.forEach((value, key) => {
      copy.append(key, value);
    });

    return copy;
  }

}
