\c postgres postgres
DROP DATABASE IF EXISTS anzu;

CREATE DATABASE anzu WITH OWNER anzu;
\c anzu postgres
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
\c anzu anzu

CREATE TABLE images (
	 id uuid PRIMARY KEY,
	 name TEXT NOT NULL,
	 image bytea NOT NULL
);

CREATE TABLE tags (
	 id uuid PRIMARY KEY,
	 name TEXT NOT NULL UNIQUE,
	 description TEXT,
	 parent uuid REFERENCES tags ON DELETE CASCADE
);

CREATE TABLE tagmap (
	 image_id uuid NOT NULL REFERENCES images ON DELETE CASCADE,
	 tag_id uuid NOT NULL REFERENCES tags ON DELETE CASCADE,
	 PRIMARY KEY (image_id, tag_id)
);

CREATE TABLE contradictions (
	 tag1_id uuid REFERENCES tags ON DELETE CASCADE,
	 tag2_id uuid REFERENCES tags ON DELETE CASCADE,
	 CHECK (tag1_id != tag2_id),
	 PRIMARY KEY (tag1_id, tag2_id),
	 UNIQUE (tag2_id, tag1_id)
);

--Add Parent Tags Automatically
CREATE FUNCTION add_parent_tags() RETURNS trigger AS $tag$
	 DECLARE pTag uuid;
	 BEGIN
		  pTag = (SELECT parent FROM tags WHERE id = NEW.tag_id);
		  IF (pTag NOTNULL) AND (NOT EXISTS (SELECT tag_id FROM tagmap WHERE tag_id = pTag AND image_id = NEW.image_id)) THEN
				INSERT INTO tagmap VALUES (NEW.image_id, pTag);
		  END IF;
		  RETURN NEW;
	 END;
$tag$ LANGUAGE plpgsql;

CREATE TRIGGER add_tag
	 BEFORE INSERT ON tagmap
	 FOR EACH ROW
	 EXECUTE PROCEDURE add_parent_tags();

--Remove Child Tags Automatically
CREATE FUNCTION remove_child_tags() RETURNS TRIGGER AS $tag$
	 DECLARE childRow record;
	 BEGIN
		  FOR childRow IN SELECT id FROM tags WHERE parent = OLD.tag_id LOOP
				DELETE FROM tagmap WHERE tag_id = childRow.id AND image_id = OLD.image_id;
		  END LOOP;
		  RETURN OLD;
	 END;
$tag$ LANGUAGE plpgsql;

CREATE TRIGGER remove_tag
	 AFTER DELETE ON tagmap
	 FOR EACH ROW
	 EXECUTE PROCEDURE remove_child_tags();

--Inforce Contradictions
CREATE FUNCTION prevent_contradictions() RETURNS TRIGGER AS $tag$
	 DECLARE cTag record;
	 BEGIN
		  FOR cTag IN (SELECT DISTINCT tag1_id,  tag2_id FROM contradictions WHERE tag1_id = NEW.tag_id OR tag2_id = NEW.tag_id)  LOOP
				IF (cTag.tag1_id = NEW.tag_id) THEN
					 IF (EXISTS (SELECT tagmap.tag_id FROM tagmap WHERE tagmap.tag_id = cTag.tag2_id AND image_id = NEW.image_id)) THEN
						  RAISE 'Contradictory Tag(name = "%", id = "%") Present', (SELECT name FROM tags WHERE id = cTag.tag2_id), cTag.tag2_id;
					 END IF;
				ELSE
					 IF (EXISTS (SELECT tagmap.tag_id FROM tagmap WHERE tagmap.tag_id = cTag.tag1_id AND image_id = NEW.image_id)) THEN
						  RAISE 'Contradictory Tag(name = "%", id = "%") Present', (SELECT name FROM tags WHERE id = cTag.tag1_id), cTag.tag1_id;
					 END IF;
				END IF;
		  END LOOP;
		  RETURN NEW;
	 END;
$tag$ LANGUAGE plpgsql;

CREATE TRIGGER prevent_contradictions
	BEFORE INSERT ON tagmap
	FOR EACH ROW
	EXECUTE PROCEDURE prevent_contradictions();

--TEST
--Tags
INSERT INTO tags VALUES('f17597f0-8e11-4189-aa9c-6dd5bb0c4afc', 'Person', 'The image contains a person');
INSERT INTO tags VALUES('947ec0e0-8108-44ce-bb34-f01b3939d243', 'Girl', 'The image contains a girl', 'f17597f0-8e11-4189-aa9c-6dd5bb0c4afc');
INSERT INTO tags VALUES('5d584d4c-b09c-4077-9cb6-191fb39b6fc6', 'Boy', 'The image contains a boy', 'f17597f0-8e11-4189-aa9c-6dd5bb0c4afc');
INSERT INTO tags VALUES('7394e2be-ddca-40d6-899e-4026342d4ffd', 'Inside', 'The image is of an indoor location');
INSERT INTO tags VALUES('c46a9af6-18c1-4848-bc0c-4761d75b98de', 'Outside', 'The image is of an outdoor location');

--Contradictions
INSERT INTO contradictions VALUES('7394e2be-ddca-40d6-899e-4026342d4ffd', 'c46a9af6-18c1-4848-bc0c-4761d75b98de');

--Image
INSERT INTO images VALUES('177fcc6e-fdb3-4f77-aaea-906fa6e878c2', 'Test', 'file://');

--Tagmap
--INSERT INTO tagmap VALUES('177fcc6e-fdb3-4f77-aaea-906fa6e878c2', 'f17597f0-8e11-4189-aa9c-6dd5bb0c4afc');
INSERT INTO tagmap VALUES('177fcc6e-fdb3-4f77-aaea-906fa6e878c2', '947ec0e0-8108-44ce-bb34-f01b3939d243');
INSERT INTO tagmap VALUES('177fcc6e-fdb3-4f77-aaea-906fa6e878c2', '5d584d4c-b09c-4077-9cb6-191fb39b6fc6');

SELECT * FROM tagmap;

--Remove Tag
DELETE FROM tagmap WHERE tag_id = 'f17597f0-8e11-4189-aa9c-6dd5bb0c4afc';

SELECT * FROM tagmap;

--Test Contradictions
INSERT INTO tagmap VALUES('177fcc6e-fdb3-4f77-aaea-906fa6e878c2', '7394e2be-ddca-40d6-899e-4026342d4ffd');
INSERT INTO tagmap VALUES('177fcc6e-fdb3-4f77-aaea-906fa6e878c2', 'c46a9af6-18c1-4848-bc0c-4761d75b98de');
