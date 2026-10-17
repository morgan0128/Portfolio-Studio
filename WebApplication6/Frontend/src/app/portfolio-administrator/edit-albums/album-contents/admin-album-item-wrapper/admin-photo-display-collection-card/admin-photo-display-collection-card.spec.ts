import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { PhotoDisplayItem } from '../../../../../models/AlbumItemDto';
import { AdminPhotoDisplayCard } from '../admin-photo-display-card/admin-photo-display-card';
import { AdminPhotoDisplayCollectionCard } from './admin-photo-display-collection-card';

describe('AdminPhotoDisplayCollectionCard', () => {
  let component: AdminPhotoDisplayCollectionCard;
  let fixture: ComponentFixture<AdminPhotoDisplayCollectionCard>;

  function photoDisplayItem(id: number): PhotoDisplayItem {
    return {
      id,
      order: id,
      content: {
        photo: { id, image: null, name: `Photo ${id}`, description: '', yearContentCreated: null },
        photoDisplayCollectionId: 301,
        displaysName: true,
        displaysDescription: true,
        displaysYearContentCreated: true,
      },
    };
  }

  function photoCard(): AdminPhotoDisplayCard {
    return fixture.debugElement.query(By.directive(AdminPhotoDisplayCard)).componentInstance;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminPhotoDisplayCollectionCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminPhotoDisplayCollectionCard);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('item', {
      id: 301,
      order: 1,
      content: { displayMode: 0, photoDisplays: [photoDisplayItem(101), photoDisplayItem(102)] },
    });
    fixture.detectChanges();
  });

  it('forwards requested changes with the photo identity captured before navigation', () => {
    const requested = jasmine.createSpy('requested');
    component.photoDisplayFieldsChangeRequested.subscribe(requested);

    photoCard().toggleDisplaysName();
    component.nextPhoto();
    fixture.detectChanges();

    expect(photoCard().item().id).toBe(102);
    expect(requested).toHaveBeenCalledOnceWith({ photoDisplayId: 101, changes: { displaysName: false } });
    expect(component.item().content.photoDisplays[0].content.displaysName).toBeTrue();
  });

  it('keeps carousel navigation and preview active while mutation requests are disabled', () => {
    const fieldsRequested = jasmine.createSpy('fieldsRequested');
    const previewRequested = jasmine.createSpy('previewRequested');
    component.photoDisplayFieldsChangeRequested.subscribe(fieldsRequested);
    component.photoDetailedViewRequest.subscribe(previewRequested);
    fixture.componentRef.setInput('mutationPending', true);
    fixture.detectChanges();

    const navigation = fixture.nativeElement.querySelectorAll('.collection-nav') as NodeListOf<HTMLButtonElement>;
    expect(Array.from(navigation).every(button => !button.disabled)).toBeTrue();
    navigation[1].click();
    fixture.detectChanges();
    expect(photoCard().item().id).toBe(102);
    expect(photoCard().mutationPending()).toBeTrue();
    photoCard().toggleDisplaysName();
    photoCard().sendPhotoViewRequest();

    expect(fieldsRequested).not.toHaveBeenCalled();
    expect(previewRequested).toHaveBeenCalledOnceWith(photoCard().photo());
  });

  it('wraps carousel navigation in either direction', () => {
    component.prevPhoto();
    expect(component.selectedPhotoDisplay()?.id).toBe(102);
    component.nextPhoto();
    expect(component.selectedPhotoDisplay()?.id).toBe(101);
    component.nextPhoto();
    component.nextPhoto();
    expect(component.selectedPhotoDisplay()?.id).toBe(101);
  });
});
