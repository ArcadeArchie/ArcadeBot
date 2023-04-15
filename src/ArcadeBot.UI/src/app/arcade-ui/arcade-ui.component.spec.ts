import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArcadeUIComponent } from './arcade-ui.component';

describe('ArcadeUIComponent', () => {
  let component: ArcadeUIComponent;
  let fixture: ComponentFixture<ArcadeUIComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ArcadeUIComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArcadeUIComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
