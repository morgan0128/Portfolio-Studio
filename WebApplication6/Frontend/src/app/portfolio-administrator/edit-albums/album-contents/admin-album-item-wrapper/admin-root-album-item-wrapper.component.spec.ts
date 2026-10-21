import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { AlbumItemDto, PhotoDisplayItem } from '../../../../models/AlbumItemDto';
import { AdminPhotoDisplayCard } from './admin-photo-display-card/admin-photo-display-card';
import { AdminRootAlbumItemWrapper } from './admin-root-album-item-wrapper.component';

describe('AdminRootAlbumItemWrapper', () => {
  let component: AdminRootAlbumItemWrapper;
  let fixture: ComponentFixture<AdminRootAlbumItemWrapper>;

  function photoDisplayItem(id: number): PhotoDisplayItem {
    return {
      id,
      order: id,
      content: {
        photo: { id, image: null, name: `Photo ${id}`, description: '', yearContentCreated: null },
        photoDisplayCollectionId: null,
        displaysName: true,
        displaysDescription: false,
        displaysYearContentCreated: true,
      },
    };
  }

  function collectionItem(): AlbumItemDto {
    return {
      id: 301,
      order: 1,
      kind: 'photoDisplayCollection',
      content: { displayMode: 0, photoDisplays: [photoDisplayItem(101), photoDisplayItem(102)] },
    };
  }

  function photoCard(): AdminPhotoDisplayCard {
    return fixture.debugElement.query(By.directive(AdminPhotoDisplayCard)).componentInstance;
  }

  function groupingButton(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('.photo-collection-grouping button');
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminRootAlbumItemWrapper]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminRootAlbumItemWrapper);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('item', { ...photoDisplayItem(101), kind: 'photoDisplay' });
    fixture.componentRef.setInput('collectionGroupingView', true);
    fixture.detectChanges();
  });

  it('forwards requested field changes without replacing the supplied item', () => {
    const requested = jasmine.createSpy('requested');
    component.photoDisplayFieldsChangeRequested.subscribe(requested);

    photoCard().toggleDisplaysName();
    photoCard().toggleDisplaysDesc();
    photoCard().toggleDisplaysYearCC();

    expect(requested.calls.allArgs()).toEqual([
      [{ photoDisplayId: 101, changes: { displaysName: false } }],
      [{ photoDisplayId: 101, changes: { displaysDescription: true } }],
      [{ photoDisplayId: 101, changes: { displaysYearContentCreated: false } }],
    ]);
    expect((component.item().content as PhotoDisplayItem['content']).displaysName).toBeTrue();
  });

  it('forwards field and preview requests from a collection photo', () => {
    fixture.componentRef.setInput('item', collectionItem());
    fixture.detectChanges();
    const fieldsRequested = jasmine.createSpy('fieldsRequested');
    const previewRequested = jasmine.createSpy('previewRequested');
    component.photoDisplayFieldsChangeRequested.subscribe(fieldsRequested);
    component.photoDetailedViewRequest.subscribe(previewRequested);

    photoCard().toggleDisplaysName();
    photoCard().sendPhotoViewRequest();

    expect(fieldsRequested).toHaveBeenCalledOnceWith({ photoDisplayId: 101, changes: { displaysName: false } });
    expect(previewRequested).toHaveBeenCalledOnceWith(photoCard().photo());
  });

  it('emits grouping requests based on its controlled selection input', () => {
    const requested = jasmine.createSpy('requested');
    component.collectionGroupingOptStatusChanged.subscribe(requested);

    groupingButton().click();
    expect(requested).toHaveBeenCalledOnceWith(true);
    expect(component.collectionGroupingOptStatus()).toBeFalse();

    fixture.componentRef.setInput('collectionGroupingOptStatus', true);
    fixture.detectChanges();
    groupingButton().click();
    expect(requested.calls.mostRecent().args).toEqual([false]);
  });

  it('requests dissolution only for an available collection', () => {
    const requested = jasmine.createSpy('requested');
    component.dissolveRequested.subscribe(requested);
    component.dissolveCollection();
    expect(requested).not.toHaveBeenCalled();

    fixture.componentRef.setInput('item', collectionItem());
    fixture.detectChanges();
    groupingButton().click();
    expect(requested).toHaveBeenCalledOnceWith(301);

    fixture.componentRef.setInput('mutationPending', true);
    fixture.detectChanges();
    expect(groupingButton().disabled).toBeTrue();
    component.dissolveCollection();
    expect(requested).toHaveBeenCalledTimes(1);
  });

  it('blocks grouping and field requests while allowing previews during a mutation', () => {
    const groupingRequested = jasmine.createSpy('groupingRequested');
    const fieldsRequested = jasmine.createSpy('fieldsRequested');
    const previewRequested = jasmine.createSpy('previewRequested');
    component.collectionGroupingOptStatusChanged.subscribe(groupingRequested);
    component.photoDisplayFieldsChangeRequested.subscribe(fieldsRequested);
    component.photoDetailedViewRequest.subscribe(previewRequested);
    fixture.componentRef.setInput('mutationPending', true);
    fixture.detectChanges();

    expect(groupingButton().disabled).toBeTrue();
    groupingButton().click();
    photoCard().toggleDisplaysName();
    photoCard().sendPhotoViewRequest();

    expect(groupingRequested).not.toHaveBeenCalled();
    expect(fieldsRequested).not.toHaveBeenCalled();
    expect(previewRequested).toHaveBeenCalledOnceWith(photoCard().photo());
  });
});
