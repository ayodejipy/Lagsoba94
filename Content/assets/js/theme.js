/* ===================
    Table Of Content
======================
01 SLICK SLIDER
02 MENU HOVER

======================*/

(function($) {
    "use strict";

    
    // 01 Slick Slider
	if($('.blog-slider').length > 0) {
		$('.blog-slider').slick({
			dots: false,
			autoplay: true,
			infinite: true,
			variableWidth: true,
		});
	}
	
	if($('.excos-slider').length > 0) {
		$('.excos-slider').slick({
			dots: false,
			autoplay:false,
			infinite: true,
			variableWidth: true,
		});
	}
	// if($('.features-slider').length > 0) {
	// 	$('.features-slider').slick({
	// 		dots: true,
	// 		infinite: true,
	// 		centerMode: true,
	// 		slidesToShow: 3,
	// 		speed: 500,
	// 		variableWidth: true,
	// 		arrows: false,
	// 		autoplay:false,
	// 		responsive: [{
	// 			  breakpoint: 992,
	// 			  settings: {
	// 				slidesToShow: 1
	// 			  }

	// 		}]
	// 	});
	// }
    // End Slider Carousel

    // 02 MENU HOVERABLE
    const $dropdown = $(".dropdown");
    const $dropdownToggle = $(".dropdown-toggle");
    const $dropdownMenu = $(".dropdown-menu");
    const showClass = "show";
    
    $(window).on("load resize", function() {
    if (this.matchMedia("(min-width: 768px)").matches) {
        $dropdown.hover(
        function() {
            const $this = $(this);
            $this.addClass(showClass);
            $this.find($dropdownToggle).attr("aria-expanded", "true");
            $this.find($dropdownMenu).addClass(showClass);
        },
        function() {
            const $this = $(this);
            $this.removeClass(showClass);
            $this.find($dropdownToggle).attr("aria-expanded", "false");
            $this.find($dropdownMenu).removeClass(showClass);
        }
        );
    } else {
        $dropdown.off("mouseenter mouseleave");
    }
    });
	// 02 MENU HOVERABLE ENDS
	
	//DOM elements
    const DOMstrings = {
	    stepsBtnClass: 'multisteps-form__progress-btn',
	    stepsBtns: document.querySelectorAll(`.multisteps-form__progress-btn`),
	    stepsBar: document.querySelector('.multisteps-form__progress'),
	    stepsForm: document.querySelector('.multisteps-form__form'),
	    stepsFormTextareas: document.querySelectorAll('.multisteps-form__textarea'),
	    stepFormPanelClass: 'multisteps-form__panel',
	    stepFormPanels: document.querySelectorAll('.multisteps-form__panel'),
	    stepPrevBtnClass: 'js-btn-prev',
        stepNextBtnClass: 'js-btn-next'
    };
  
  
  //remove class from a set of items
  const removeClasses = (elemSet, className) => {
  
	elemSet.forEach(elem => {
  
	  elem.classList.remove(className);
  
	});
  
  };
  
  //return exect parent node of the element
  const findParent = (elem, parentClass) => {
  
	let currentNode = elem;
  
	while (!currentNode.classList.contains(parentClass)) {
	  currentNode = currentNode.parentNode;
	}
  
	return currentNode;
  
  };
  
  //get active button step number
  const getActiveStep = elem => {
	return Array.from(DOMstrings.stepsBtns).indexOf(elem);
  };
  
  //set all steps before clicked (and clicked too) to active
  const setActiveStep = activeStepNum => {
  
	//remove active state from all the state
	removeClasses(DOMstrings.stepsBtns, 'js-active');
  
	//set picked items to active
	DOMstrings.stepsBtns.forEach((elem, index) => {
  
	  if (index <= activeStepNum) {
		elem.classList.add('js-active');
	  }
  
	});
  };
  
  //get active panel
  const getActivePanel = () => {
  
	let activePanel;
  
	DOMstrings.stepFormPanels.forEach(elem => {
  
	  if (elem.classList.contains('js-active')) {
  
		activePanel = elem;
  
	  }
  
	});
  
	return activePanel;
  
  };
  
  //open active panel (and close unactive panels)
  const setActivePanel = activePanelNum => {
  
	//remove active class from all the panels
	removeClasses(DOMstrings.stepFormPanels, 'js-active');
  
	//show active panel
	DOMstrings.stepFormPanels.forEach((elem, index) => {
	  if (index === activePanelNum) {
  
		elem.classList.add('js-active');
  
		setFormHeight(elem);
  
	  }
	});
  
  };
  
  //set form height equal to current panel height
  const formHeight = activePanel => {
  
	const activePanelHeight = activePanel.offsetHeight;
  
	DOMstrings.stepsForm.style.height = `${activePanelHeight}px`;
  
  };
  
  const setFormHeight = () => {
	const activePanel = getActivePanel();
  
	formHeight(activePanel);
  };
  
  //STEPS BAR CLICK FUNCTION
  DOMstrings.stepsBar.addEventListener('click', e => {
  
	//check if click target is a step button
	const eventTarget = e.target;
  
	if (!eventTarget.classList.contains(`${DOMstrings.stepsBtnClass}`)) {
	  return;
	}
  
	//get active button step number
	const activeStep = getActiveStep(eventTarget);
  
	//set all steps before clicked (and clicked too) to active
	setActiveStep(activeStep);
  
	//open active panel
	setActivePanel(activeStep);
  });
  
  //PREV/NEXT BTNS CLICK
  DOMstrings.stepsForm.addEventListener('click', e => {
  
	const eventTarget = e.target;
  
	//check if we clicked on `PREV` or NEXT` buttons
	if (!(eventTarget.classList.contains(`${DOMstrings.stepPrevBtnClass}`) || eventTarget.classList.contains(`${DOMstrings.stepNextBtnClass}`)))
	{
	  return;
	}
  
	//find active panel
	const activePanel = findParent(eventTarget, `${DOMstrings.stepFormPanelClass}`);
  
	let activePanelNum = Array.from(DOMstrings.stepFormPanels).indexOf(activePanel);
  
	//set active step and active panel onclick
	if (eventTarget.classList.contains(`${DOMstrings.stepPrevBtnClass}`)) {
	  activePanelNum--;
  
	} else {
  
	  activePanelNum++;
  
	}
  
	setActiveStep(activePanelNum);
	setActivePanel(activePanelNum);
  
  });
  
  //SETTING PROPER FORM HEIGHT ONLOAD
  window.addEventListener('load', setFormHeight, false);
  
  //SETTING PROPER FORM HEIGHT ONRESIZE
  window.addEventListener('resize', setFormHeight, false);

    // Call the dataTables jQuery plugin
    $(document).ready(function () {
        $('#dataTable').DataTable({
            "language": {
                "paginate": {
                    "next": '<i class="fas fa-angle-right"></i>',
                    "previous": '<i class="fas fa-angle-left"></i>'
                }
            }
        });
    });

  
})(jQuery);


